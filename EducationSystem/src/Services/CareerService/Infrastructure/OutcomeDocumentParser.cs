using CareerService.Application;

namespace CareerService.Infrastructure;

public sealed class OutcomeDocumentParser(
    OpenXmlOutcomeDocumentParser docxParser,
    PdfOutcomeDocumentParser pdfParser,
    IPdfOcrService pdfOcrService) : IOutcomeDocumentParser
{
    public async Task<IReadOnlyList<DocumentBlockData>> ParseAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken)
    {
        if (string.Equals(
                contentType,
                DocxFileValidator.ContentType,
                StringComparison.OrdinalIgnoreCase))
            return await docxParser.ParseAsync(stream, cancellationToken);
        if (!string.Equals(
                contentType,
                PdfFileValidator.ContentType,
                StringComparison.OrdinalIgnoreCase))
            throw new OutcomeImportException(
                "INVALID_CONTENT_TYPE",
                "The stored document MIME type is not supported.",
                StatusCodes.Status400BadRequest);

        await using var copy = new MemoryStream();
        await stream.CopyToAsync(copy, cancellationToken);
        var pdf = copy.ToArray();
        var blocks = await pdfParser.ParseAsync(
            new MemoryStream(pdf, writable: false),
            cancellationToken);
        return blocks.Count > 0
            ? blocks
            : await pdfOcrService.RecognizeAsync(pdf, cancellationToken);
    }
}
