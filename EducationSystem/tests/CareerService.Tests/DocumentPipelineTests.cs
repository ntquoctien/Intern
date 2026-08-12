using CareerService.Application;
using CareerService.Infrastructure;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CareerService.Tests;

public sealed class DocumentPipelineTests
{
    [Fact]
    public async Task ValidDocx_IsAccepted_AndParsedIntoTraceableBlocks()
    {
        var bytes = CreateDocx();
        var validator = new DocxFileValidator(Options.Create(new OutcomeStorageOptions()));
        validator.Validate("outcomes.docx", DocxFileValidator.ContentType, bytes);

        var blocks = await new OpenXmlOutcomeDocumentParser()
            .ParseAsync(new MemoryStream(bytes), CancellationToken.None);

        Assert.Contains(blocks, block => block.BlockId == "heading-0" && block.HeadingLevel == 1);
        Assert.Contains(blocks, block => block.BlockType == "Table");
        Assert.Contains(blocks, block => block.BlockType == "TableCell" && block.ContentJson.Contains("PLO1"));
        Assert.Equal(blocks.Count, blocks.Select(block => block.BlockId).Distinct().Count());
    }

    [Fact]
    public void InvalidArchive_IsRejectedBeforeStorage()
    {
        var validator = new DocxFileValidator(Options.Create(new OutcomeStorageOptions()));
        var error = Assert.Throws<OutcomeImportException>(() =>
            validator.Validate("outcomes.docx", DocxFileValidator.ContentType, "not a zip"u8.ToArray()));
        Assert.Equal("INVALID_DOCX", error.ErrorCode);
    }

    [Fact]
    public void ValidDocx_WithGenericBrowserMimeType_IsAccepted()
    {
        var validator = new DocxFileValidator(
            Options.Create(new OutcomeStorageOptions()));

        validator.Validate(
            "outcomes.docx",
            "application/octet-stream",
            CreateDocx());
    }

    [Fact]
    public void ValidPdf_IsAcceptedByDocumentValidator()
    {
        var options = Options.Create(new OutcomeStorageOptions());
        var validator = new OutcomeDocumentFileValidator(
            new DocxFileValidator(options),
            new PdfFileValidator(options));

        var extension = validator.Validate(
            "outcomes.pdf",
            PdfFileValidator.ContentType,
            "%PDF-1.7\n%%EOF"u8.ToArray());

        Assert.Equal(".pdf", extension);
    }

    [Fact]
    public void PdfWithMismatchedMimeType_IsRejected()
    {
        var validator = new PdfFileValidator(
            Options.Create(new OutcomeStorageOptions()));

        var error = Assert.Throws<OutcomeImportException>(() => validator.Validate(
            "outcomes.pdf",
            DocxFileValidator.ContentType,
            "%PDF-1.7\n%%EOF"u8.ToArray()));

        Assert.Equal("INVALID_CONTENT_TYPE", error.ErrorCode);
    }

    [Fact]
    public async Task ImportProblem_IncludesExistingBatchDetails()
    {
        var middleware = new OutcomeExceptionMiddleware(
            _ => throw new OutcomeImportException(
                "DUPLICATE_IMPORT",
                "Document already exists.",
                StatusCodes.Status409Conflict,
                extensions: new Dictionary<string, object?>
                {
                    ["existingImportId"] = 42L,
                    ["existingImportStatus"] = "PendingReview"
                }),
            NullLogger<OutcomeExceptionMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var problem = await JsonDocument.ParseAsync(context.Response.Body);
        Assert.Equal(409, context.Response.StatusCode);
        Assert.Equal(42, problem.RootElement.GetProperty("existingImportId").GetInt64());
        Assert.Equal(
            "PendingReview",
            problem.RootElement.GetProperty("existingImportStatus").GetString());
    }

    [Fact]
    public async Task LocalStorage_DoesNotAllowKeysOutsideItsRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), $"career-storage-{Guid.NewGuid():N}");
        try
        {
            var storage = new LocalOutcomeFileStorage(
                Options.Create(new OutcomeStorageOptions { RootPath = root }),
                new FakeEnvironment(root));
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                storage.SaveAsync("../escape.docx", new byte[] { 1 }, CancellationToken.None));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static byte[] CreateDocx()
    {
        using var memory = new MemoryStream();
        using (var document = WordprocessingDocument.Create(memory, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
        {
            var main = document.AddMainDocumentPart();
            main.Document = new Document(new Body(
                new Paragraph(
                    new ParagraphProperties(new ParagraphStyleId { Val = "Heading1" }),
                    new Run(new Text("Chuẩn đầu ra chương trình"))),
                new Paragraph(new Run(new Text("Nội dung mô tả"))),
                new Table(
                    new TableRow(
                        new TableCell(new Paragraph(new Run(new Text("Mã")))),
                        new TableCell(new Paragraph(new Run(new Text("Mô tả"))))),
                    new TableRow(
                        new TableCell(new Paragraph(new Run(new Text("PLO1")))),
                        new TableCell(new Paragraph(new Run(new Text("Vận dụng kiến thức"))))))));
            main.Document.Save();
        }
        return memory.ToArray();
    }

    private sealed class FakeEnvironment(string contentRootPath) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "CareerService.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = contentRootPath;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
