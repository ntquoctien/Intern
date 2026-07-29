using System.Net;
using System.Text;
using CareerService.Application;
using CareerService.Infrastructure;
using CareerService.Infrastructure.Persistence;
using CareerService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CareerService.Tests;

public sealed class OutcomeValidationTests
{
    [Fact]
    public async Task Validation_BlocksUnknownSourcesMappingsAndExistingOfficialOutcomes()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseInMemoryDatabase($"career-{Guid.NewGuid():N}").Options;
        await using var db = new CareerDbContext(options);
        db.CurriculumVersions.Add(new CurriculumVersion
        {
            Id = 7, MajorCode = "SE", MajorName = "Kỹ thuật phần mềm",
            CurriculumCode = "SE-2026", Version = "1", Status = "Draft",
            RowVersion = [1], CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        db.OutcomeDocumentBlocks.Add(new OutcomeDocumentBlock
        {
            ImportBatchId = 11, BlockId = "paragraph-1", BlockType = "Paragraph",
            Sequence = 1, ContentJson = "{}", CreatedAt = DateTime.UtcNow,
        });
        db.OutcomeImportBatches.Add(new OutcomeImportBatch
        {
            Id = 11,
            CurriculumVersionId = 7,
            SelectedSubjectExternalId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            SelectedSubjectCode = "CS999",
            SelectedSubjectName = "Selected subject",
            OriginalFileName = "outcomes.docx",
            StorageKey = "test/outcomes.docx",
            ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            FileSize = 1,
            FileHash = new string('a', 64),
            Status = OutcomeImportStatuses.PendingReview,
            ProgressPercent = 100,
            UploadedByExternalId = "test",
            UploadedByName = "Test",
            RowVersion = [1],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
        db.ProgramLearningOutcomes.Add(new ProgramLearningOutcome
        {
            CurriculumVersionId = 7, PloCode = "OLD", Description = "Existing",
            SortOrder = 1, Status = "Approved", RowVersion = [1],
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var academic = new AcademicSubjectClient(
            new HttpClient(new JsonHandler("""{"data":[{"subjectId":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","subjectCode":"CS101","name":"Programming","creditPoint":3}]}""")),
            Options.Create(new AcademicClientOptions()),
            NullLogger<AcademicSubjectClient>.Instance);
        var service = new OutcomeValidationService(db, academic);
        var known = new SourceReference("paragraph-1", "Paragraph", 1, [], 1, null, null, null, "text");
        var unknown = known with { BlockId = "invented-block" };
        var document = new OutcomeDraftDocument
        {
            ImportBatchId = 11,
            Curriculum = new CurriculumDraft
            {
                DraftId = "curriculum", MajorCode = "SE", CurriculumCode = "SE-2026",
                Version = "1", Confidence = .9, SourceReferences = [known],
            },
            Plos =
            [
                new PloDraft { DraftId = "plo1", PloCode = "PLO1", Description = "Outcome", Confidence = .9, SourceReferences = [known] }
            ],
            Subjects =
            [
                new SubjectDraft
                {
                    DraftId = "subject1", Confidence = .9, SourceReferences = [known],
                    Subject = new SubjectReference { Code = "CS101", Name = "Programming", Credits = 3 },
                    Clos = [new CloDraft { DraftId = "clo1", CloCode = "CLO1", Description = "Outcome", Confidence = .9, SourceReferences = [unknown] }],
                }
            ],
            Mappings =
            [
                new MappingDraft { DraftId = "map1", CloDraftId = "missing", PloDraftId = "plo1", ProgressionLevel = "X", Confidence = .9, SourceReferences = [known] }
            ],
        };

        var issues = await service.ValidateAsync(document, 7, CancellationToken.None);

        Assert.Contains(issues, issue => issue.Code == "SOURCE_REFERENCE_NOT_FOUND");
        Assert.Contains(issues, issue => issue.Code == "MAPPING_TARGET_NOT_FOUND");
        Assert.Contains(issues, issue => issue.Code == "INVALID_PROGRESSION_LEVEL");
        Assert.Contains(issues, issue => issue.Code == "CURRICULUM_ALREADY_HAS_OUTCOMES");
        Assert.Contains(issues, issue => issue.Code == "SELECTED_SUBJECT_MISMATCH");
        Assert.Contains(issues, issue => issue.Code == "UNEXPECTED_ADDITIONAL_SUBJECT");
        Assert.Equal("Matched", document.Subjects[0].Subject.MatchStatus);
    }

    [Fact]
    public void RowVersionCodec_RejectsMalformedConcurrencyToken()
    {
        var error = Assert.Throws<OutcomeImportException>(() => RowVersionCodec.Decode("not-base64!"));
        Assert.Equal("INVALID_ROW_VERSION", error.ErrorCode);
    }

    private sealed class JsonHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });
    }
}
