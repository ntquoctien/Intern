using CareerService.Infrastructure;
using CareerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CareerService.Application;

public sealed class OutcomeValidationService(
    CareerDbContext dbContext,
    AcademicSubjectClient academicClient) : IOutcomeValidationService
{
    public async Task<IReadOnlyList<OutcomeWarning>> ValidateAsync(
        OutcomeDraftDocument document,
        long curriculumVersionId,
        CancellationToken cancellationToken)
    {
        var result = new List<OutcomeWarning>();
        var curriculum = await dbContext.CurriculumVersions.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == curriculumVersionId, cancellationToken);
        if (curriculum is null)
        {
            result.Add(Block("CURRICULUM_NOT_FOUND", null, null, "Target curriculum was not found."));
            return result;
        }

        var selectedSubject = document.ImportBatchId <= 0
            ? null
            : await dbContext.OutcomeImportBatches.AsNoTracking()
                .Where(item => item.Id == document.ImportBatchId)
                .Select(item => new
                {
                    item.SelectedSubjectExternalId,
                    item.SelectedSubjectCode,
                    item.SelectedSubjectName
                })
                .SingleOrDefaultAsync(cancellationToken);

        if (curriculum.Status is not ("Draft" or "PendingReview"))
            result.Add(Block(
                "CURRICULUM_NOT_IMPORTABLE", document.Curriculum.DraftId, null,
                "Only Draft or PendingReview curriculum versions may be imported."));

        CompareMetadata(result, document.Curriculum, curriculum.MajorCode,
            document.Curriculum.MajorCode, "majorCode", "MAJOR_CODE_MISMATCH");
        CompareMetadata(result, document.Curriculum, curriculum.CurriculumCode,
            document.Curriculum.CurriculumCode, "curriculumCode", "CURRICULUM_CODE_MISMATCH");
        CompareMetadata(result, document.Curriculum, curriculum.Version,
            document.Curriculum.Version, "version", "VERSION_MISMATCH");

        if (!string.IsNullOrWhiteSpace(document.Curriculum.MajorName) &&
            !string.Equals(curriculum.MajorName.Trim(), document.Curriculum.MajorName.Trim(),
                StringComparison.OrdinalIgnoreCase))
            result.Add(Warn(
                "MAJOR_NAME_MISMATCH", document.Curriculum.DraftId, "majorName",
                "Document major name differs from the selected curriculum snapshot."));

        var allDraftIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var knownBlockIdList = await dbContext.OutcomeDocumentBlocks.AsNoTracking()
            .Where(item => item.ImportBatchId == document.ImportBatchId)
            .Select(item => item.BlockId)
            .ToListAsync(cancellationToken);
        var knownBlockIds = knownBlockIdList.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in EnumerateDraftItems(document))
        {
            if (string.IsNullOrWhiteSpace(item.DraftId) || !allDraftIds.Add(item.DraftId))
                result.Add(Block("INVALID_DRAFT_ID", item.DraftId, "draftId",
                    "Draft IDs must be non-empty and unique."));
            if (item.Confidence is < 0 or > 1)
                result.Add(Block("INVALID_CONFIDENCE", item.DraftId, "confidence",
                    "Confidence must be between 0 and 1."));
            if (!item.Removed && item.SourceReferences.Count == 0)
                result.Add(Block("SOURCE_REFERENCE_REQUIRED", item.DraftId, "sourceReferences",
                    "Every active extracted item must reference a document block."));
            foreach (var source in item.SourceReferences)
                if (string.IsNullOrWhiteSpace(source.BlockId) ||
                    !knownBlockIds.Contains(source.BlockId))
                    result.Add(Block(
                        "SOURCE_REFERENCE_NOT_FOUND", item.DraftId, "sourceReferences",
                        $"Source block {source.BlockId} does not exist in this import."));
            if (!item.Removed && item.Confidence < 0.6)
                result.Add(Warn("LOW_CONFIDENCE", item.DraftId, "confidence",
                    "The extraction confidence is low and should be reviewed."));
        }

        foreach (var plo in document.Plos.Where(item => !item.Removed))
        {
            Required(result, plo.PloCode, plo.DraftId, "ploCode", "PLO_CODE_REQUIRED");
            Required(result, plo.Description, plo.DraftId, "description", "PLO_DESCRIPTION_REQUIRED");
            Maximum(result, plo.PloCode, 50, plo.DraftId, "ploCode", "PLO_CODE_TOO_LONG");
        }
        AddDuplicateCodeErrors(
            document.Plos.Where(item => !item.Removed),
            item => item.PloCode,
            item => item.DraftId,
            "DUPLICATE_PLO_CODE",
            "PLO code",
            result);

        var subjects = await academicClient.GetSubjectsAsync(cancellationToken);
        AddDuplicateCodeErrors(
            document.Subjects.Where(item => !item.Removed),
            item => item.Subject.Code,
            item => item.DraftId,
            "DUPLICATE_SUBJECT_CODE",
            "subject code",
            result);
        foreach (var subject in document.Subjects.Where(item => !item.Removed))
        {
            Required(result, subject.Subject.Code, subject.DraftId, "subject.code", "SUBJECT_CODE_REQUIRED");
            Maximum(result, subject.Subject.Code, 100, subject.DraftId, "subject.code", "SUBJECT_CODE_TOO_LONG");
            Maximum(result, subject.Subject.Name, 255, subject.DraftId, "subject.name", "SUBJECT_NAME_TOO_LONG");
            if (subjects.TryGetValue(subject.Subject.Code.Trim(), out var match))
            {
                subject.Subject.ExternalId = match.Id;
                subject.Subject.MatchStatus = "Matched";
                if (string.IsNullOrWhiteSpace(subject.Subject.Name)) subject.Subject.Name = match.Name;
                if (!subject.Subject.Credits.HasValue) subject.Subject.Credits = match.Credits;
            }
            else
            {
                subject.Subject.MatchStatus = subjects.Count == 0 ? "NotChecked" : "NotFound";
                result.Add(Block(
                    subjects.Count == 0 ? "SUBJECT_LOOKUP_UNAVAILABLE" : "SUBJECT_NOT_FOUND",
                    subject.DraftId, "subject.code",
                    subjects.Count == 0
                        ? "Academic subject lookup is unavailable; validate again when AcademicService is online."
                        : $"Subject {subject.Subject.Code} was not found in academic data."));
            }

            if (subject.Subject.Credits is <= 0)
                result.Add(Block("INVALID_CREDITS", subject.DraftId, "subject.credits",
                    "Credits must be greater than zero when supplied."));

            foreach (var clo in subject.Clos.Where(item => !item.Removed))
            {
                Required(result, clo.CloCode, clo.DraftId, "cloCode", "CLO_CODE_REQUIRED");
                Required(result, clo.Description, clo.DraftId, "description", "CLO_DESCRIPTION_REQUIRED");
                Maximum(result, clo.CloCode, 50, clo.DraftId, "cloCode", "CLO_CODE_TOO_LONG");
            }
            AddDuplicateCodeErrors(
                subject.Clos.Where(item => !item.Removed),
                item => item.CloCode,
                item => item.DraftId,
                "DUPLICATE_CLO_CODE",
                $"CLO code in subject {subject.Subject.Code}",
                result);
        }

        if (!string.IsNullOrWhiteSpace(selectedSubject?.SelectedSubjectCode))
        {
            var activeSubjects = document.Subjects.Where(item => !item.Removed).ToList();
            var selectedDraft = activeSubjects.FirstOrDefault(item =>
                string.Equals(
                    item.Subject.Code?.Trim(),
                    selectedSubject.SelectedSubjectCode.Trim(),
                    StringComparison.OrdinalIgnoreCase));
            if (selectedDraft is null)
                result.Add(Block(
                    "SELECTED_SUBJECT_MISMATCH", null, "subject.code",
                    $"The extracted draft does not contain selected subject {selectedSubject.SelectedSubjectCode}."));
            else if (selectedSubject.SelectedSubjectExternalId.HasValue &&
                     selectedDraft.Subject.ExternalId.HasValue &&
                     selectedDraft.Subject.ExternalId != selectedSubject.SelectedSubjectExternalId)
                result.Add(Block(
                    "SELECTED_SUBJECT_ID_MISMATCH", selectedDraft.DraftId, "subject.externalId",
                    "The extracted subject resolves to a different academic subject."));

            foreach (var additional in activeSubjects.Where(item =>
                         !string.Equals(
                             item.Subject.Code?.Trim(),
                             selectedSubject.SelectedSubjectCode.Trim(),
                             StringComparison.OrdinalIgnoreCase)))
                result.Add(Warn(
                    "UNEXPECTED_ADDITIONAL_SUBJECT", additional.DraftId, "subject.code",
                    $"Subject {additional.Subject.Code} was not selected for this import."));
        }

        var activePloIds = document.Plos.Where(item => !item.Removed)
            .Select(item => item.DraftId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var activeCloIds = document.Subjects.Where(item => !item.Removed)
            .SelectMany(item => item.Clos).Where(item => !item.Removed)
            .Select(item => item.DraftId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in document.Mappings.Where(item => !item.Removed))
        {
            if (!activeCloIds.Contains(mapping.CloDraftId) || !activePloIds.Contains(mapping.PloDraftId))
                result.Add(Block("MAPPING_TARGET_NOT_FOUND", mapping.DraftId, null,
                    "Mapping references a missing or removed CLO/PLO."));
            if (mapping.ProgressionLevel is not ("E" or "R" or "D"))
                result.Add(Block("INVALID_PROGRESSION_LEVEL", mapping.DraftId, "progressionLevel",
                    "Progression level must be E, R or D."));
        }

        var existing = await dbContext.ProgramLearningOutcomes.AsNoTracking()
            .AnyAsync(item => item.CurriculumVersionId == curriculumVersionId, cancellationToken)
            || await dbContext.CourseLearningOutcomes.AsNoTracking()
                .AnyAsync(item => item.CurriculumVersionId == curriculumVersionId, cancellationToken)
            || await dbContext.CloPloMappings.AsNoTracking()
                .AnyAsync(item => item.CurriculumVersionId == curriculumVersionId, cancellationToken);
        if (existing)
            result.Add(Block("CURRICULUM_ALREADY_HAS_OUTCOMES", null, null,
                "Insert-only approval is blocked because the curriculum already has official outcomes."));

        result.AddRange(document.Warnings);
        AddCoverageWarnings(document, result);
        return result;
    }

    private static IEnumerable<DraftItem> EnumerateDraftItems(OutcomeDraftDocument document)
    {
        yield return document.Curriculum;
        foreach (var plo in document.Plos) yield return plo;
        foreach (var subject in document.Subjects)
        {
            yield return subject;
            foreach (var clo in subject.Clos) yield return clo;
        }
        foreach (var mapping in document.Mappings) yield return mapping;
    }

    private static void CompareMetadata(
        ICollection<OutcomeWarning> result,
        CurriculumDraft draft,
        string expected,
        string? actual,
        string field,
        string code)
    {
        if (string.IsNullOrWhiteSpace(actual))
            result.Add(Warn("DOCUMENT_METADATA_MISSING", draft.DraftId, field,
                $"Document metadata {field} was not found."));
        else if (!string.Equals(expected.Trim(), actual.Trim(), StringComparison.OrdinalIgnoreCase))
            result.Add(Block(code, draft.DraftId, field,
                $"Document {field} does not match the selected curriculum."));
    }

    private static void Required(
        ICollection<OutcomeWarning> result,
        string? value,
        string draftId,
        string field,
        string code)
    {
        if (string.IsNullOrWhiteSpace(value))
            result.Add(Block(code, draftId, field, $"{field} is required."));
    }

    private static void Maximum(
        ICollection<OutcomeWarning> result,
        string? value,
        int maximum,
        string draftId,
        string field,
        string code)
    {
        if (value?.Length > maximum)
            result.Add(Block(code, draftId, field, $"{field} cannot exceed {maximum} characters."));
    }

    private static void AddCoverageWarnings(
        OutcomeDraftDocument document,
        ICollection<OutcomeWarning> result)
    {
        var mappings = document.Mappings.Where(item => !item.Removed).ToList();
        foreach (var clo in document.Subjects.SelectMany(item => item.Clos).Where(item => !item.Removed))
            if (!mappings.Any(item => item.CloDraftId.Equals(clo.DraftId, StringComparison.OrdinalIgnoreCase)))
                result.Add(Warn("CLO_WITHOUT_PLO", clo.DraftId, null, "CLO has no PLO mapping."));
        foreach (var plo in document.Plos.Where(item => !item.Removed))
            if (!mappings.Any(item => item.PloDraftId.Equals(plo.DraftId, StringComparison.OrdinalIgnoreCase)))
                result.Add(Warn("PLO_WITHOUT_CLO", plo.DraftId, null, "PLO receives no CLO contribution."));
    }

    private static void AddDuplicateCodeErrors<T>(
        IEnumerable<T> items,
        Func<T, string> code,
        Func<T, string> draftId,
        string errorCode,
        string label,
        ICollection<OutcomeWarning> result)
    {
        foreach (var group in items
                     .Where(item => !string.IsNullOrWhiteSpace(code(item)))
                     .GroupBy(item => code(item).Trim(), StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1))
            foreach (var duplicate in group)
                result.Add(Block(
                    errorCode, draftId(duplicate), "code",
                    $"{label} {group.Key} must be unique."));
    }

    private static OutcomeWarning Block(string code, string? id, string? field, string message) =>
        new(code, "BlockingError", id, field, message);
    private static OutcomeWarning Warn(string code, string? id, string? field, string message) =>
        new(code, "Warning", id, field, message);
}
