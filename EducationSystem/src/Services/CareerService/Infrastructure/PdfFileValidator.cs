using CareerService.Application;
using Microsoft.Extensions.Options;

namespace CareerService.Infrastructure;

public sealed class PdfFileValidator(IOptions<OutcomeStorageOptions> options)
{
    public const string ContentType = "application/pdf";

    public void Validate(
        string fileName,
        string contentType,
        ReadOnlyMemory<byte> content)
    {
        if (!string.Equals(
                Path.GetExtension(fileName),
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
            throw new OutcomeImportException(
                "INVALID_FILE_EXTENSION",
                "The uploaded file must have a .pdf extension.",
                StatusCodes.Status400BadRequest);
        if (!IsExpectedContentType(contentType))
            throw new OutcomeImportException(
                "INVALID_CONTENT_TYPE",
                "The PDF MIME type must be application/pdf.",
                StatusCodes.Status400BadRequest);
        if (content.Length == 0)
            throw new OutcomeImportException(
                "EMPTY_FILE",
                "The uploaded PDF is empty.",
                StatusCodes.Status400BadRequest);
        if (content.Length > options.Value.MaxFileSizeBytes)
            throw new OutcomeImportException(
                "FILE_TOO_LARGE",
                $"The uploaded file exceeds the {options.Value.MaxFileSizeBytes} byte limit.",
                StatusCodes.Status400BadRequest);
        if (content.Length < 5 || !content.Span[..5].SequenceEqual("%PDF-"u8))
            throw new OutcomeImportException(
                "INVALID_PDF",
                "The uploaded file does not contain a valid PDF signature.",
                StatusCodes.Status400BadRequest);
    }

    private static bool IsExpectedContentType(string contentType) =>
        string.IsNullOrWhiteSpace(contentType) ||
        string.Equals(contentType, ContentType, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase);
}
