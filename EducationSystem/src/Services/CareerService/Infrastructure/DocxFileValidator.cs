using System.IO.Compression;
using CareerService.Application;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Options;

namespace CareerService.Infrastructure;

public sealed class DocxFileValidator(IOptions<OutcomeStorageOptions> options)
{
    public const string ContentType =
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public void Validate(string fileName, string contentType, ReadOnlyMemory<byte> content)
    {
        var limits = options.Value;
        if (!string.Equals(Path.GetExtension(fileName), ".docx", StringComparison.OrdinalIgnoreCase))
            throw new OutcomeImportException("INVALID_FILE_EXTENSION", "Only .docx files are accepted.", 400);
        if (!IsExpectedContentType(contentType))
            throw new OutcomeImportException("INVALID_CONTENT_TYPE", "The file MIME type is not DOCX.", 400);
        if (content.Length == 0 || content.Length > limits.MaxFileSizeBytes)
            throw new OutcomeImportException("INVALID_FILE_SIZE", $"DOCX size must be between 1 and {limits.MaxFileSizeBytes} bytes.", 400);

        try
        {
            using var memory = new MemoryStream(content.ToArray(), writable: false);
            using (var archive = new ZipArchive(memory, ZipArchiveMode.Read, leaveOpen: true))
            {
                if (archive.Entries.Count > limits.MaxZipEntries)
                    throw new OutcomeImportException("DOCX_TOO_MANY_ENTRIES", "The DOCX contains too many ZIP entries.", 400);
                long expanded = 0;
                foreach (var entry in archive.Entries)
                {
                    expanded = checked(expanded + entry.Length);
                    if (expanded > limits.MaxExpandedSizeBytes)
                        throw new OutcomeImportException("DOCX_EXPANDED_SIZE_EXCEEDED", "The expanded DOCX is too large.", 400);
                    var normalized = entry.FullName.Replace('\\', '/');
                    if (normalized.StartsWith('/') || normalized.Split('/').Any(part => part == ".."))
                        throw new OutcomeImportException("DOCX_PATH_TRAVERSAL", "The DOCX contains an unsafe ZIP path.", 400);
                }
            }

            memory.Position = 0;
            using var document = WordprocessingDocument.Open(memory, false);
            if (document.MainDocumentPart?.Document.Body is null)
                throw new OutcomeImportException("INVALID_DOCX", "The DOCX has no document body.", 400);
        }
        catch (OutcomeImportException)
        {
            throw;
        }
        catch (Exception exception) when (
            exception is InvalidDataException or OpenXmlPackageException or IOException)
        {
            throw new OutcomeImportException("INVALID_DOCX", "The DOCX package is invalid or damaged.", 400, exception);
        }
    }

    private static bool IsExpectedContentType(string contentType) =>
        string.IsNullOrWhiteSpace(contentType) ||
        string.Equals(contentType, ContentType, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(contentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase);
}
