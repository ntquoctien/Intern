using System.Text.Json;

namespace CareerService.Application;

public sealed class OutcomeReconciliationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public OutcomeDraftDocument Reconcile(
        long batchId,
        string programJson,
        string outcomesJson,
        string mappingsJson,
        string? selectedSubjectCode = null)
    {
        var program = JsonSerializer.Deserialize<ProgramExtraction>(programJson, JsonOptions)
            ?? new ProgramExtraction();
        var outcomes = JsonSerializer.Deserialize<OutcomesExtraction>(outcomesJson, JsonOptions)
            ?? new OutcomesExtraction();
        var mappings = JsonSerializer.Deserialize<MappingsExtraction>(mappingsJson, JsonOptions)
            ?? new MappingsExtraction();

        var document = new OutcomeDraftDocument
        {
            ImportBatchId = batchId,
            Curriculum = program.Curriculum ?? new CurriculumDraft(),
            Plos = program.Plos ?? [],
            Subjects = outcomes.Subjects ?? [],
            Mappings = mappings.Mappings ?? [],
            Warnings =
            [
                .. program.Warnings ?? [],
                .. outcomes.Warnings ?? [],
                .. mappings.Warnings ?? []
            ]
        };

        EnsureDraftIds(document);
        ConstrainToSelectedSubject(document, selectedSubjectCode);
        NormalizeValues(document);
        Deduplicate(document);
        return document;
    }

    private static void ConstrainToSelectedSubject(
        OutcomeDraftDocument document,
        string? selectedSubjectCode)
    {
        if (Blank(selectedSubjectCode))
            return;

        var normalizedSelectedSubjectCode = selectedSubjectCode!.Trim();
        var selectedSubjects = document.Subjects
            .Where(item =>
                string.Equals(
                    item.Subject.Code?.Trim(),
                    normalizedSelectedSubjectCode,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();

        // The provider's raw JSON remains available for audit. The review document is
        // deliberately scoped to the subject selected by the administrator.
        if (selectedSubjects.Count == 0)
            return;

        foreach (var removedSubject in document.Subjects.Except(selectedSubjects))
            document.Warnings.Add(new OutcomeWarning(
                "OUT_OF_SCOPE_SUBJECT_REMOVED",
                "Warning",
                removedSubject.DraftId,
                "subject.code",
                $"Provider-returned subject {removedSubject.Subject.Code} was removed because this import is scoped to {normalizedSelectedSubjectCode}."));

        document.Subjects = selectedSubjects;
        var selectedCloIds = selectedSubjects
            .SelectMany(item => item.Clos)
            .Select(item => item.DraftId)
            .Where(item => !Blank(item))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        document.Mappings = document.Mappings
            .Where(item => selectedCloIds.Contains(item.CloDraftId))
            .ToList();
    }

    private static void NormalizeValues(OutcomeDraftDocument document)
    {
        foreach (var plo in document.Plos)
            plo.PloCode = plo.PloCode.Trim().ToUpperInvariant();
        foreach (var subject in document.Subjects)
        {
            subject.Subject.Code = subject.Subject.Code.Trim().ToUpperInvariant();
            subject.Subject.Name = subject.Subject.Name.Trim();
            foreach (var clo in subject.Clos)
                clo.CloCode = clo.CloCode.Trim().ToUpperInvariant();
        }
        foreach (var mapping in document.Mappings)
        {
            mapping.CloDraftId = mapping.CloDraftId.Trim();
            mapping.PloDraftId = mapping.PloDraftId.Trim();
            mapping.ProgressionLevel = mapping.ProgressionLevel.Trim().ToUpperInvariant();
        }
    }

    private static void EnsureDraftIds(OutcomeDraftDocument document)
    {
        document.Curriculum.DraftId =
            Blank(document.Curriculum.DraftId) ? "curriculum-001" : document.Curriculum.DraftId;

        for (var index = 0; index < document.Plos.Count; index++)
        {
            if (Blank(document.Plos[index].DraftId))
                document.Plos[index].DraftId = $"plo-{index + 1:000}";
        }

        for (var subjectIndex = 0; subjectIndex < document.Subjects.Count; subjectIndex++)
        {
            var subject = document.Subjects[subjectIndex];
            if (Blank(subject.DraftId)) subject.DraftId = $"subject-{subjectIndex + 1:000}";
            for (var cloIndex = 0; cloIndex < subject.Clos.Count; cloIndex++)
            {
                if (Blank(subject.Clos[cloIndex].DraftId))
                    subject.Clos[cloIndex].DraftId = $"clo-{subjectIndex + 1:000}-{cloIndex + 1:000}";
            }
        }

        for (var index = 0; index < document.Mappings.Count; index++)
        {
            if (Blank(document.Mappings[index].DraftId))
                document.Mappings[index].DraftId = $"mapping-{index + 1:000}";
        }
    }

    private static void Deduplicate(OutcomeDraftDocument document)
    {
        foreach (var group in document.Plos
                     .Where(item => !item.Removed)
                     .GroupBy(item => item.PloCode.Trim(), StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1))
        {
            if (group.Select(item => Normalize(item.Description)).Distinct().Count() > 1)
                document.Warnings.Add(new OutcomeWarning(
                    "PLO_CONTENT_CONFLICT", "BlockingError", group.First().DraftId,
                    "description", $"PLO {group.Key} appears with conflicting descriptions."));
            else
                foreach (var duplicate in group.Skip(1)) duplicate.Removed = true;
        }

        foreach (var subject in document.Subjects.Where(item => !item.Removed))
        {
            foreach (var group in subject.Clos
                         .Where(item => !item.Removed)
                         .GroupBy(item => item.CloCode.Trim(), StringComparer.OrdinalIgnoreCase)
                         .Where(group => group.Count() > 1))
            {
                if (group.Select(item => Normalize(item.Description)).Distinct().Count() > 1)
                    document.Warnings.Add(new OutcomeWarning(
                        "CLO_CONTENT_CONFLICT", "BlockingError", group.First().DraftId,
                        "description",
                        $"{subject.Subject.Code} {group.Key} appears with conflicting descriptions."));
                else
                    foreach (var duplicate in group.Skip(1)) duplicate.Removed = true;
            }
        }

        foreach (var group in document.Mappings
                     .Where(item => !item.Removed)
                     .GroupBy(
                         item => $"{item.CloDraftId}\u001f{item.PloDraftId}",
                         StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1))
        {
            var values = group.Select(item => item.ProgressionLevel.Trim().ToUpperInvariant()).Distinct().ToList();
            if (values.Count > 1)
                document.Warnings.Add(new OutcomeWarning(
                    "MAPPING_LEVEL_CONFLICT", "BlockingError", group.First().DraftId,
                    "progressionLevel", "The same CLO-PLO pair contains conflicting E/R/D values."));
            else
            {
                foreach (var duplicate in group.Skip(1)) duplicate.Removed = true;
                document.Warnings.Add(new OutcomeWarning(
                    "DUPLICATE_MAPPING", "Warning", group.First().DraftId,
                    null, "A duplicate CLO-PLO mapping was collapsed."));
            }
        }
    }

    private static bool Blank(string? value) => string.IsNullOrWhiteSpace(value);
    private static string Normalize(string value) =>
        string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToUpperInvariant();

    private sealed class ProgramExtraction
    {
        public CurriculumDraft? Curriculum { get; set; }
        public List<PloDraft>? Plos { get; set; }
        public List<OutcomeWarning>? Warnings { get; set; }
    }

    private sealed class OutcomesExtraction
    {
        public List<SubjectDraft>? Subjects { get; set; }
        public List<OutcomeWarning>? Warnings { get; set; }
    }

    private sealed class MappingsExtraction
    {
        public List<MappingDraft>? Mappings { get; set; }
        public List<OutcomeWarning>? Warnings { get; set; }
    }
}
