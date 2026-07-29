using System.Text.Json;
using CareerService.Application;
using CareerService.Infrastructure.Persistence;
using CareerService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerService.Tests;

public sealed class ApprovedOutcomeDocumentTests
{
    [Fact]
    public void CanonicalContent_IsStableAcrossInputOrdering()
    {
        var curriculum = Curriculum();
        var firstPlos = new[]
        {
            Plo(2, "PLO2"),
            Plo(1, "PLO1")
        };
        var firstClos = new[]
        {
            Clo(2, "CLO2"),
            Clo(1, "CLO1")
        };
        var firstMappings = new[]
        {
            new ApprovedMappingContent("CS101", "CLO2", "PLO2", "D"),
            new ApprovedMappingContent("CS101", "CLO1", "PLO1", "E")
        };

        var first = ApprovedOutcomeDocumentService.BuildCanonicalContent(
            curriculum, firstPlos, firstClos, firstMappings);
        var second = ApprovedOutcomeDocumentService.BuildCanonicalContent(
            curriculum, firstPlos.Reverse(), firstClos.Reverse(), firstMappings.Reverse());
        var firstJson = ApprovedOutcomeDocumentService.SerializeCanonical(first);
        var secondJson = ApprovedOutcomeDocumentService.SerializeCanonical(second);

        Assert.Equal(firstJson, secondJson);
        Assert.Equal(
            ApprovedOutcomeDocumentService.Hash(firstJson),
            ApprovedOutcomeDocumentService.Hash(secondJson));
        Assert.DoesNotContain("draftId", firstJson);
        Assert.DoesNotContain("confidence", firstJson);
        Assert.DoesNotContain("warnings", firstJson);
    }

    [Fact]
    public async Task Materialize_ReadsCanonicalSql_VersionsAndSupersedesDocuments()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseInMemoryDatabase($"approved-doc-{Guid.NewGuid():N}").Options;
        await using var db = new CareerDbContext(options);
        db.CurriculumVersions.Add(Curriculum());
        db.OutcomeImportBatches.AddRange(Batch(1), Batch(2));
        db.ProgramLearningOutcomes.Add(Plo(1, "PLO1"));
        db.CourseLearningOutcomes.Add(Clo(1, "CLO1"));
        await db.SaveChangesAsync();
        db.CloPloMappings.Add(new CloPloMapping
        {
            Id = 1,
            CurriculumVersionId = 7,
            CloId = 1,
            PloId = 1,
            ProgressionLevelCode = "E",
            IsApproved = true,
            RowVersion = [1]
        });
        await db.SaveChangesAsync();

        var service = new ApprovedOutcomeDocumentService(
            db, new FixedTimeProvider(new DateTimeOffset(2026, 7, 29, 0, 0, 0, TimeSpan.Zero)));
        var first = await service.MaterializeAsync(
            7, 1, new ActorContext("admin", "Administrator"), CancellationToken.None);
        var second = await service.MaterializeAsync(
            7, 2, new ActorContext("admin", "Administrator"), CancellationToken.None);

        Assert.Equal(1, first.DocumentVersion);
        Assert.Equal(ApprovedOutcomeDocumentStatuses.Superseded, first.Status);
        Assert.NotNull(first.SupersededAt);
        Assert.Equal(2, second.DocumentVersion);
        Assert.Equal(ApprovedOutcomeDocumentStatuses.Active, second.Status);
        Assert.Equal(first.ContentHash, second.ContentHash);
        using var content = JsonDocument.Parse(second.ContentJson);
        Assert.Equal("SE-2026", content.RootElement.GetProperty("curriculum")
            .GetProperty("curriculumCode").GetString());
        Assert.Equal("PLO1", content.RootElement.GetProperty("plos")[0]
            .GetProperty("ploCode").GetString());
        Assert.Equal("CLO1", content.RootElement.GetProperty("subjects")[0]
            .GetProperty("clos")[0].GetProperty("cloCode").GetString());
        Assert.Equal("E", content.RootElement.GetProperty("mappings")[0]
            .GetProperty("progressionLevel").GetString());
        Assert.DoesNotContain("STAGING-ONLY", second.ContentJson);
    }

    private static CurriculumVersion Curriculum() => new()
    {
        Id = 7,
        MajorCode = "SE",
        MajorName = "Software Engineering",
        CurriculumCode = "SE-2026",
        CurriculumName = "Software Engineering 2026",
        Version = "1",
        Status = "Draft",
        RowVersion = [1]
    };

    private static ProgramLearningOutcome Plo(int order, string code) => new()
    {
        Id = order,
        CurriculumVersionId = 7,
        PloCode = code,
        Description = $"Description {code}",
        SortOrder = order,
        Status = "Approved",
        RowVersion = [1]
    };

    private static CourseLearningOutcome Clo(int order, string code) => new()
    {
        Id = order,
        CurriculumVersionId = 7,
        SubjectExternalId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        SubjectCode = "CS101",
        SubjectName = "Programming",
        Credits = 3,
        CloCode = code,
        Description = $"Description {code}",
        SortOrder = order,
        Status = "Approved",
        RowVersion = [1]
    };

    private static OutcomeImportBatch Batch(long id) => new()
    {
        Id = id,
        CurriculumVersionId = 7,
        OriginalFileName = $"{id}.docx",
        StorageKey = $"{id}.docx",
        ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        FileSize = 1,
        FileHash = new string((char)('a' + id - 1), 64),
        ReviewedJson = """{"staging":"STAGING-ONLY"}""",
        Status = OutcomeImportStatuses.PendingReview,
        ProgressPercent = 100,
        UploadedByExternalId = "admin",
        UploadedByName = "Administrator",
        RowVersion = [1]
    };

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
