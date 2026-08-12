using System.Text.Json;

namespace CareerService.Application;

public sealed record OutcomeExtractionSelection(
    string MajorCode,
    string MajorName,
    string CurriculumCode,
    string? CurriculumName,
    string Version,
    Guid? SubjectExternalId,
    string? SubjectCode,
    string? SubjectName);

public static class OutcomeExtractionContextBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly string[] ProgramKeywords =
    [
        "program learning outcome", "chuẩn đầu ra", "mục tiêu chương trình",
        "mục tiêu đào tạo", "\"plo", "plo "
    ];

    private static readonly string[] CourseKeywords =
    [
        "course learning outcome", "chuẩn đầu ra môn", "chuẩn đầu ra học phần",
        "cđr môn học", "cđr học phần", "mục tiêu môn học", "mục tiêu học phần",
        "về kiến thức", "về kỹ năng", "năng lực tự chủ", "\"clo", "clo "
    ];

    private static readonly string[] MatrixKeywords =
    [
        "clo-plo", "clo – plo", "clo–plo", "ma trận", "matrix"
    ];

    public static string BuildDocumentContext(
        int requestNumber,
        OutcomeExtractionSelection selection,
        IReadOnlyList<DocumentBlockData> blocks,
        int maximumCharacters)
    {
        var candidates = SelectBlocks(blocks, requestNumber);
        return Fit(
            candidates,
            selectedBlocks => JsonSerializer.Serialize(new
            {
                contextScope = requestNumber == 1 ? "ProgramOutcomes" : "CourseOutcomes",
                selectedCurriculum = Curriculum(selection),
                selectedSubject = Subject(selection),
                blocks = selectedBlocks.Select(Block)
            }, JsonOptions),
            maximumCharacters);
    }

    public static string BuildMatrixContext(
        OutcomeExtractionSelection selection,
        IReadOnlyList<DocumentBlockData> blocks,
        string programJson,
        string outcomesJson,
        int maximumCharacters)
    {
        using var program = JsonDocument.Parse(programJson);
        using var outcomes = JsonDocument.Parse(outcomesJson);
        var candidates = SelectBlocks(blocks, 3);
        return Fit(
            candidates,
            selectedBlocks => JsonSerializer.Serialize(new
            {
                contextScope = "ExplicitCloPloMatrix",
                selectedCurriculum = Curriculum(selection),
                selectedSubject = Subject(selection),
                blocks = selectedBlocks.Select(Block),
                extractedProgram = program.RootElement,
                extractedOutcomes = outcomes.RootElement
            }, JsonOptions),
            maximumCharacters);
    }

    public static bool HasExplicitMatrixEvidence(IReadOnlyList<DocumentBlockData> blocks) =>
        SelectBlocks(blocks, 3).Count > 0;

    public static IReadOnlyList<DocumentBlockData> GetFormalCourseObjectives(
        IReadOnlyList<DocumentBlockData> blocks) =>
        SelectCourseObjectiveSection(
                blocks.Where(item => item.BlockType is "Heading" or "Paragraph" or "Table")
                    .OrderBy(item => item.Sequence)
                    .ToList())
            .Where(item =>
            {
                try
                {
                    using var content = JsonDocument.Parse(item.ContentJson);
                    if (!content.RootElement.TryGetProperty("text", out var text))
                        return false;
                    var value = text.GetString()?.TrimStart();
                    return value?.StartsWith('+') == true ||
                           value?.StartsWith('•') == true;
                }
                catch (JsonException)
                {
                    return false;
                }
            })
            .ToList();

    public static int CountFormalCourseObjectives(IReadOnlyList<DocumentBlockData> blocks) =>
        GetFormalCourseObjectives(blocks).Count;

    private static IReadOnlyList<DocumentBlockData> SelectBlocks(
        IReadOnlyList<DocumentBlockData> blocks,
        int requestNumber)
    {
        var eligible = blocks
            .Where(item => item.BlockType is "Heading" or "Paragraph" or "Table")
            .OrderBy(item => item.Sequence)
            .ToList();
        if (requestNumber == 2)
        {
            var outcomeSection = SelectCourseOutcomeSection(eligible);
            if (outcomeSection.Count > 0)
                return outcomeSection;
        }
        var selected = new HashSet<int>();

        if (requestNumber is 1 or 2)
            for (var index = 0; index < Math.Min(30, eligible.Count); index++)
                selected.Add(index);

        var keywords = requestNumber switch
        {
            1 => ProgramKeywords,
            2 => CourseKeywords,
            _ => MatrixKeywords
        };
        for (var index = 0; index < eligible.Count; index++)
        {
            var searchable = SearchableText(eligible[index].ContentJson);
            if (!keywords.Any(searchable.Contains)) continue;
            for (var neighbor = Math.Max(0, index - 2);
                 neighbor <= Math.Min(eligible.Count - 1, index + 4);
                 neighbor++)
                selected.Add(neighbor);
        }

        return selected.Order().Select(index => eligible[index]).ToList();
    }

    private static IReadOnlyList<DocumentBlockData> SelectCourseObjectiveSection(
        IReadOnlyList<DocumentBlockData> blocks)
    {
        var start = -1;
        for (var index = 0; index < blocks.Count; index++)
        {
            var searchable = SearchableText(blocks[index].ContentJson);
            if (start < 0 && searchable.Contains("mục tiêu môn học"))
            {
                start = index;
                continue;
            }
            if (start >= 0 && searchable.Contains("nội dung môn học"))
                return blocks.Skip(start).Take(index - start).ToList();
        }
        return [];
    }

    private static IReadOnlyList<DocumentBlockData> SelectCourseOutcomeSection(
        IReadOnlyList<DocumentBlockData> blocks)
    {
        var start = -1;
        for (var index = 0; index < blocks.Count; index++)
        {
            var searchable = SearchableText(blocks[index].ContentJson);
            if (start < 0 &&
                (searchable.Contains("chuẩn đầu ra học phần") ||
                 searchable.Contains("chuẩn đầu ra môn học") ||
                 searchable.Contains("cđr học phần") ||
                 searchable.Contains("cđr môn học")))
            {
                start = index;
                continue;
            }
            if (start >= 0 &&
                (searchable.Contains("nội dung chi tiết học phần") ||
                 searchable.Contains("nội dung môn học") ||
                 searchable.Contains("hướng dẫn tổ chức dạy học")))
                return blocks.Skip(start).Take(index - start).ToList();
        }

        if (start >= 0)
            return blocks.Skip(start).ToList();

        return SelectCourseObjectiveSection(blocks);
    }

    private static string SearchableText(string contentJson)
    {
        try
        {
            using var document = JsonDocument.Parse(contentJson);
            var values = new List<string>();
            CollectStrings(document.RootElement, values);
            return string.Join(' ', values).ToLowerInvariant();
        }
        catch (JsonException)
        {
            return contentJson.ToLowerInvariant();
        }
    }

    private static void CollectStrings(JsonElement element, ICollection<string> values)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                values.Add(element.GetString() ?? string.Empty);
                break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                    CollectStrings(property.Value, values);
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    CollectStrings(item, values);
                break;
        }
    }

    private static string Fit(
        IReadOnlyList<DocumentBlockData> candidates,
        Func<IReadOnlyList<DocumentBlockData>, string> serialize,
        int maximumCharacters)
    {
        if (maximumCharacters < 1)
            throw new OutcomeImportException(
                "LLM_INPUT_LIMIT_INVALID", "The configured LLM input limit is invalid.", 503);

        var selected = new List<DocumentBlockData>();
        var current = serialize(selected);
        if (current.Length > maximumCharacters)
            throw new OutcomeImportException(
                "LLM_INPUT_TOO_LARGE",
                "Extracted draft context exceeds the configured per-request input limit.", 422);

        foreach (var block in candidates)
        {
            var candidate = serialize([.. selected, block]);
            if (candidate.Length > maximumCharacters) continue;
            selected.Add(block);
            current = candidate;
        }
        return current;
    }

    private static object Curriculum(OutcomeExtractionSelection selection) => new
    {
        selection.MajorCode,
        selection.MajorName,
        selection.CurriculumCode,
        selection.CurriculumName,
        selection.Version
    };

    private static object? Subject(OutcomeExtractionSelection selection) =>
        selection.SubjectCode is null
            ? null
            : new
            {
                externalId = selection.SubjectExternalId,
                code = selection.SubjectCode,
                name = selection.SubjectName
            };

    private static object Block(DocumentBlockData item)
    {
        var content = JsonSerializer.Deserialize<JsonElement>(item.ContentJson);
        if (item.BlockType is "Heading" or "Paragraph" &&
            content.TryGetProperty("text", out var text))
            return new
            {
                item.BlockId,
                item.BlockType,
                item.Sequence,
                content = new
                {
                    text = text.GetString(),
                    paragraphIndex = content.TryGetProperty("paragraphIndex", out var index)
                        ? index.GetInt32()
                        : (int?)null
                }
            };
        return new
        {
            item.BlockId,
            item.BlockType,
            item.Sequence,
            content
        };
    }
}
