using CommunicationService.Application.Services;
using CommunicationService.Infrastructure.Persistence;
using CommunicationService.Infrastructure.Persistence.Entities;
using ExamService.Application.Services;
using ExamService.Infrastructure.Persistence;
using ExamService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace StudentAccess.Tests;

public sealed class ExamCommunicationOwnershipTests
{
    [Fact]
    public async Task ExamResults_PreserveNullAndMultipleRawValues_AndHideForeignIds()
    {
        await using var db = NewExamDb();
        var studentA = Guid.NewGuid();
        var studentB = Guid.NewGuid();
        var exam = new SubjectTeachingExam
        {
            Id = Guid.NewGuid(), SubjectTeachingId = Guid.NewGuid(), Name = "Thi cuối kỳ",
            StartDate = new DateTime(2026, 7, 20), EndDate = new DateTime(2026, 7, 20, 2, 0, 0),
            Notes = string.Empty
        };
        var first = NewResult(studentA, exam, null, null);
        var second = NewResult(studentA, exam, 8.5f, 7.75f);
        var foreign = NewResult(studentB, exam, 10f, 10f);
        db.AddRange(exam, first, second, foreign);
        await db.SaveChangesAsync();
        var service = new StudentExamResultService(db);

        var owned = await service.GetAllAsync(studentA, default);
        Assert.Equal(2, owned.Count);
        Assert.Contains(owned, result => result.RawResult is null && result.RawCombinedResult is null);
        Assert.Null(await service.GetByIdAsync(studentA, foreign.Id, default));
        Assert.NotNull(await service.GetByIdAsync(studentA, first.Id, default));
    }

    [Fact]
    public async Task FormRequests_ReturnEmptyAndHideForeignRequestId()
    {
        await using var db = NewCommunicationDb();
        var studentA = Guid.NewGuid();
        var studentB = Guid.NewGuid();
        var template = new FormTemplate { Id = Guid.NewGuid(), Name = "Xác nhận sinh viên" };
        var foreign = new FormRequest
        {
            Id = Guid.NewGuid(), StudentId = studentB, FormTemplate = template, FormTemplateId = template.Id,
            CreationDate = new DateTime(2026, 7, 1), UpdateDate = new DateTime(2026, 7, 2),
            ApprovalName = string.Empty, Note = string.Empty
        };
        db.AddRange(template, foreign);
        await db.SaveChangesAsync();
        var service = new StudentFormRequestService(db);

        Assert.Empty(await service.GetAllAsync(studentA, default));
        Assert.Null(await service.GetByIdAsync(studentA, foreign.Id, default));
        Assert.Single(await service.GetAllAsync(studentB, default));
    }

    private static ExamResult NewResult(
        Guid studentId,
        SubjectTeachingExam exam,
        float? rawResult,
        float? rawCombined) => new()
    {
        Id = Guid.NewGuid(), StudentId = studentId, SubjectTeachingExam = exam,
        SubjectTeachingExamId = exam.Id, Result = rawResult, CombinedResult = rawCombined
    };

    private static ExamDbContext NewExamDb() => new(
        new DbContextOptionsBuilder<ExamDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static CommunicationDbContext NewCommunicationDb() => new(
        new DbContextOptionsBuilder<CommunicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
