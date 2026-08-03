using System.Text.Json;
using CareerService.Application;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace CareerService.Infrastructure;

public sealed class OpenXmlOutcomeDocumentParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Task<IReadOnlyList<DocumentBlockData>> ParseAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var document = WordprocessingDocument.Open(stream, false);
        var body = document.MainDocumentPart?.Document.Body
            ?? throw new OutcomeImportException(
                "INVALID_DOCX", "The DOCX does not contain a document body.", 400);

        var blocks = new List<DocumentBlockData>();
        var headings = new List<string>();
        var sequence = 0;
        var paragraphIndex = 0;
        var tableIndex = 0;

        foreach (var element in body.Elements())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (element is Paragraph paragraph)
            {
                var text = Normalize(paragraph.InnerText);
                if (text.Length == 0)
                {
                    paragraphIndex++;
                    continue;
                }

                var level = HeadingLevel(paragraph);
                if (level.HasValue)
                {
                    while (headings.Count >= level.Value) headings.RemoveAt(headings.Count - 1);
                    while (headings.Count < level.Value - 1) headings.Add(string.Empty);
                    headings.Add(text);
                }

                var blockId = level.HasValue
                    ? $"heading-{paragraphIndex}"
                    : $"paragraph-{paragraphIndex}";
                blocks.Add(new DocumentBlockData(
                    blockId,
                    level.HasValue ? "Heading" : "Paragraph",
                    sequence++,
                    level,
                    JsonSerializer.Serialize(new
                    {
                        text,
                        paragraphIndex,
                        headingPath = headings.Where(item => item.Length > 0).ToArray()
                    }, JsonOptions)));
                paragraphIndex++;
            }
            else if (element is Table table)
            {
                ParseTable(table, tableIndex++, headings, blocks, ref sequence);
            }
        }

        return Task.FromResult<IReadOnlyList<DocumentBlockData>>(blocks);
    }

    private static void ParseTable(
        Table table,
        int tableIndex,
        IReadOnlyList<string> headings,
        ICollection<DocumentBlockData> blocks,
        ref int sequence)
    {
        var rows = table.Elements<TableRow>().ToList();
        var tableRows = rows.Select((row, rowIndex) =>
            row.Elements<TableCell>().Select((cell, columnIndex) => new
            {
                rowIndex,
                columnIndex,
                text = Normalize(cell.InnerText),
                gridSpan = cell.TableCellProperties?.GridSpan?.Val?.Value,
                verticalMerge = cell.TableCellProperties?.VerticalMerge?.Val?.Value.ToString()
            }).ToArray()).ToArray();

        blocks.Add(new DocumentBlockData(
            $"table-{tableIndex}",
            "Table",
            sequence++,
            null,
            JsonSerializer.Serialize(new
            {
                tableIndex,
                headingPath = headings.Where(item => item.Length > 0).ToArray(),
                rows = tableRows
            }, JsonOptions)));

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var cells = rows[rowIndex].Elements<TableCell>().ToList();
            blocks.Add(new DocumentBlockData(
                $"table-{tableIndex}-row-{rowIndex}",
                "TableRow",
                sequence++,
                null,
                JsonSerializer.Serialize(new
                {
                    tableIndex,
                    rowIndex,
                    headingPath = headings.Where(item => item.Length > 0).ToArray(),
                    cells = cells.Select((cell, columnIndex) => new
                    {
                        columnIndex,
                        text = Normalize(cell.InnerText)
                    })
                }, JsonOptions)));

            for (var columnIndex = 0; columnIndex < cells.Count; columnIndex++)
            {
                var cell = cells[columnIndex];
                blocks.Add(new DocumentBlockData(
                    $"table-{tableIndex}-row-{rowIndex}-cell-{columnIndex}",
                    "TableCell",
                    sequence++,
                    null,
                    JsonSerializer.Serialize(new
                    {
                        tableIndex,
                        rowIndex,
                        columnIndex,
                        text = Normalize(cell.InnerText),
                        headingPath = headings.Where(item => item.Length > 0).ToArray(),
                        gridSpan = cell.TableCellProperties?.GridSpan?.Val?.Value,
                        verticalMerge = cell.TableCellProperties?.VerticalMerge?.Val?.Value.ToString()
                    }, JsonOptions)));
            }
        }
    }

    private static byte? HeadingLevel(Paragraph paragraph)
    {
        var style = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
        if (!string.IsNullOrWhiteSpace(style) &&
            style.StartsWith("Heading", StringComparison.OrdinalIgnoreCase) &&
            byte.TryParse(new string(style.Where(char.IsDigit).ToArray()), out var parsed))
            return (byte)Math.Clamp((int)parsed, 1, 9);

        var outline = paragraph.ParagraphProperties?.OutlineLevel?.Val?.Value;
        return outline is null ? null : (byte)Math.Clamp((int)outline.Value + 1, 1, 9);
    }

    private static string Normalize(string? value) =>
        string.Join(' ', (value ?? string.Empty)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
