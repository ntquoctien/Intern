using System.Text.Json;
using System.Text.RegularExpressions;

namespace CareerService.Application;

public sealed partial class OutcomeQualityEvaluator
{
    public ExtractionQualityReport Evaluate(
        OutcomeDraftDocument document,
        IReadOnlyList<DocumentBlockData> blocks,
        IReadOnlyList<OutcomeWarning> validation,
        string? selectedSubjectCode)
    {
        var blockText = blocks.ToDictionary(
            item => item.BlockId,
            item => ExtractText(item.ContentJson),
            StringComparer.OrdinalIgnoreCase);
        var activePlos = document.Plos.Where(item => !item.Removed).ToList();
        var activeSubjects = document.Subjects.Where(item => !item.Removed).ToList();
        var activeClos = activeSubjects.SelectMany(item => item.Clos)
            .Where(item => !item.Removed).ToList();
        var activeMappings = document.Mappings.Where(item => !item.Removed).ToList();
        var evidenceItems = activePlos.Cast<DraftItem>()
            .Concat(activeSubjects)
            .Concat(activeClos)
            .Concat(activeMappings)
            .ToList();
        var itemsWithEvidence = evidenceItems.Count(item =>
            item.SourceReferences.Any(reference => blockText.ContainsKey(reference.BlockId)));

        var exactCloMatches = activeClos.Count(item =>
            ReferencedText(item, blockText).Contains(item.CloCode, StringComparison.OrdinalIgnoreCase));
        var exactPloMatches = activePlos.Count(item =>
            ReferencedText(item, blockText).Contains(item.PloCode, StringComparison.OrdinalIgnoreCase));
        var overlapValues = activePlos
            .Select(item => TextOverlap(item.Description, ReferencedText(item, blockText)))
            .Concat(activeClos.Select(item =>
                TextOverlap(item.Description, ReferencedText(item, blockText))))
            .ToList();
        var removedOutOfScope = document.Warnings.Count(item =>
            item.Code == "OUT_OF_SCOPE_SUBJECT_REMOVED");
        var unexpectedSubjects = validation.Count(item =>
            item.Code == "UNEXPECTED_ADDITIONAL_SUBJECT");
        var danglingMappings = validation.Count(item =>
            item.Code == "MAPPING_TARGET_NOT_FOUND");
        var duplicateMappings = document.Warnings.Count(item =>
                item.Code is "DUPLICATE_MAPPING" or "MAPPING_LEVEL_CONFLICT")
            + validation.Count(item => item.Code == "DUPLICATE_MAPPING");
        var blocking = validation
            .Where(item => item.Severity.StartsWith("Blocking", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();
        var warnings = validation
            .Where(item => !item.Severity.StartsWith("Blocking", StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToList();

        var formalSourceCloCount =
            OutcomeExtractionContextBuilder.CountFormalCourseObjectives(blocks);
        return new ExtractionQualityReport(
            ScopeCompliance(activeSubjects, selectedSubjectCode),
            Ratio(itemsWithEvidence, evidenceItems.Count),
            formalSourceCloCount > 0
                ? formalSourceCloCount
                : activeClos.Count(item =>
                    item.SourceReferences.Any(reference => blockText.ContainsKey(reference.BlockId))),
            activeClos.Count,
            Ratio(exactCloMatches, activeClos.Count),
            Ratio(exactPloMatches, activePlos.Count),
            overlapValues.Count == 0 ? 1 : Round(overlapValues.Average()),
            removedOutOfScope + unexpectedSubjects,
            removedOutOfScope,
            danglingMappings,
            duplicateMappings,
            blocking,
            warnings);
    }

    private static double ScopeCompliance(
        IReadOnlyList<SubjectDraft> subjects,
        string? selectedSubjectCode)
    {
        if (string.IsNullOrWhiteSpace(selectedSubjectCode))
            return 1;
        if (subjects.Count == 0)
            return 0;
        var inScope = subjects.Count(item =>
            string.Equals(
                item.Subject.Code.Trim(),
                selectedSubjectCode.Trim(),
                StringComparison.OrdinalIgnoreCase));
        return Ratio(inScope, subjects.Count);
    }

    private static string ReferencedText(
        DraftItem item,
        IReadOnlyDictionary<string, string> blockText) =>
        string.Join(
            ' ',
            item.SourceReferences
                .Select(reference =>
                    blockText.TryGetValue(reference.BlockId, out var text) ? text : string.Empty)
                .Where(text => text.Length > 0));

    private static string ExtractText(string contentJson)
    {
        try
        {
            using var document = JsonDocument.Parse(contentJson);
            var values = new List<string>();
            CollectStrings(document.RootElement, values);
            return string.Join(' ', values);
        }
        catch (JsonException)
        {
            return contentJson;
        }
    }

    private static void CollectStrings(JsonElement element, ICollection<string> values)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                values.Add(element.GetString() ?? string.Empty);
                break;
            case JsonValueKind.Array:
                foreach (var child in element.EnumerateArray())
                    CollectStrings(child, values);
                break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                    CollectStrings(property.Value, values);
                break;
        }
    }

    private static double TextOverlap(string extracted, string source)
    {
        var extractedTokens = Tokens(extracted);
        if (extractedTokens.Count == 0)
            return 1;
        var sourceTokens = Tokens(source);
        return Round((double)extractedTokens.Count(sourceTokens.Contains) / extractedTokens.Count);
    }

    private static HashSet<string> Tokens(string value) =>
        WordRegex().Matches(value.Normalize().ToLowerInvariant())
            .Select(match => match.Value)
            .ToHashSet(StringComparer.Ordinal);

    private static double Ratio(int numerator, int denominator) =>
        denominator == 0 ? 1 : Round((double)numerator / denominator);

    private static double Round(double value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);

    [GeneratedRegex(@"[\p{L}\p{Nd}]+", RegexOptions.CultureInvariant)]
    private static partial Regex WordRegex();
}
