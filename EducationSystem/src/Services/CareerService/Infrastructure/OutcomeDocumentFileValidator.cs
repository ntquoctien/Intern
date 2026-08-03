using CareerService.Application;

namespace CareerService.Infrastructure;

public sealed class OutcomeDocumentFileValidator(
    DocxFileValidator docxValidator,
    PdfFileValidator pdfValidator)
{
    public string Validate(
        string fileName,
        string contentType,
        ReadOnlyMemory<byte> content)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        switch (extension)
        {
            case ".docx":
                docxValidator.Validate(fileName, contentType, content);
                return extension;
            case ".pdf":
                pdfValidator.Validate(fileName, contentType, content);
                return extension;
            default:
                throw new OutcomeImportException(
                    "INVALID_FILE_EXTENSION",
                    "Only .docx and .pdf files are accepted.",
                    StatusCodes.Status400BadRequest);
        }
    }
}
