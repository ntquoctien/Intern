using ExamService.Application.DTOs.Management;
using ExamService.Application.Interfaces;
using ExamService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExamService.Application.Services;

public sealed class ManagementExamReadService(ExamDbContext dbContext) : IManagementExamReadService
{
    public async Task<RawExamResultSummaryDto> GetResultSummaryAsync(CancellationToken cancellationToken)
    {
        var all = dbContext.ExamResults.AsNoTracking().Where(result => !result.IsDeleted);
        var values = all.Where(result => result.Result.HasValue).Select(result => result.Result!.Value);
        return new RawExamResultSummaryDto(
            await all.CountAsync(cancellationToken),
            await values.CountAsync(cancellationToken),
            await values.Select(value => (float?)value).MinAsync(cancellationToken),
            await values.Select(value => (float?)value).MaxAsync(cancellationToken),
            await values.Select(value => (double?)value).AverageAsync(cancellationToken));
    }

    public async Task<IReadOnlyList<ManagementRawExamResultDto>> GetResultsAsync(CancellationToken cancellationToken) =>
        await dbContext.ExamResults.AsNoTracking()
            .Where(result => !result.IsDeleted && !result.SubjectTeachingExam.IsDeleted)
            .OrderByDescending(result => result.SubjectTeachingExam.StartDate)
            .Select(result => new ManagementRawExamResultDto(result.Id, result.StudentId,
                result.SubjectTeachingExam.Name, result.SubjectTeachingExam.StartDate,
                result.Result, result.CombinedResult, result.ExamResultDesc))
            .Take(1000)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<QuestionSuiteSummaryDto>> GetSuitesAsync(CancellationToken cancellationToken) =>
        await dbContext.QuestionSuites.AsNoTracking()
            .OrderBy(suite => suite.Name)
            .Select(suite => new QuestionSuiteSummaryDto(suite.Id, suite.SubjectId, suite.Name,
                suite.CreationTime, suite.Questions.Count, suite.UpdatedById,
                suite.Questions.Count(question => question.Level == 0),
                suite.Questions.Count(question => question.Level == 1),
                suite.Questions.Count(question => question.Level == 2),
                suite.Questions.Count(question => question.Level == 3)))
            .ToListAsync(cancellationToken);

    public Task<QuestionSuiteDetailDto?> GetSuiteAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.QuestionSuites.AsNoTracking()
            .Where(suite => suite.Id == id)
            .Select(suite => new QuestionSuiteDetailDto(suite.Id, suite.SubjectId, suite.Name,
                suite.CreationTime, suite.Questions.OrderBy(question => question.Id)
                    .Select(question => new QuestionPreviewDto(question.Id, question.QuestionText,
                        question.Level, question.ImageUrl,
                        question.QuestionAnswers.OrderBy(answer => answer.Id)
                            .Select(answer => new QuestionAnswerPreviewDto(answer.Id,
                                answer.AnswerText, answer.ImageUrl, answer.IsAnswer)).ToList()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ManagementExamPageDto> GetExamPageAsync(ManagementExamQueryDto query, CancellationToken cancellationToken)
    {
        var source = ApplyExamFilters(dbContext.SubjectTeachingExams.AsNoTracking().Where(item => !item.IsDeleted), query);
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ProjectExams(source.OrderByDescending(item => item.StartDate)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize))
            .ToListAsync(cancellationToken);
        return new ManagementExamPageDto(items, pageNumber, pageSize, totalItems,
            await dbContext.ExamAttempts.CountAsync(item => !item.IsDeleted, cancellationToken),
            await dbContext.ExamResults.CountAsync(item => !item.IsDeleted, cancellationToken),
            await dbContext.ExamResults.CountAsync(item => !item.IsDeleted && item.Result.HasValue, cancellationToken));
    }

    public Task<ManagementExamItemDto?> GetExamAsync(Guid id, CancellationToken cancellationToken) =>
        ProjectExams(dbContext.SubjectTeachingExams.AsNoTracking().Where(item => item.Id == id && !item.IsDeleted))
            .SingleOrDefaultAsync(cancellationToken);

    private static IQueryable<Infrastructure.Persistence.Entities.SubjectTeachingExam> ApplyExamFilters(
        IQueryable<Infrastructure.Persistence.Entities.SubjectTeachingExam> source, ManagementExamQueryDto query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(item => item.Name.Contains(search) || item.Notes.Contains(search));
        }
        if (query.SubjectTeachingId.HasValue) source = source.Where(item => item.SubjectTeachingId == query.SubjectTeachingId);
        if (query.Type.HasValue) source = source.Where(item => item.Type == query.Type);
        if (query.FromDate.HasValue) source = source.Where(item => item.StartDate >= query.FromDate.Value);
        if (query.ToDate.HasValue) source = source.Where(item => item.StartDate < query.ToDate.Value.AddDays(1));
        return source;
    }

    private static IQueryable<ManagementExamItemDto> ProjectExams(
        IQueryable<Infrastructure.Persistence.Entities.SubjectTeachingExam> source) =>
        source.Select(item => new ManagementExamItemDto(item.Id, item.SubjectTeachingId,
            item.QuestionSuiteId, item.QuestionSuite != null ? item.QuestionSuite.Name : null,
            item.Name, item.StartDate, item.EndDate, item.RoomId, item.TeacherId, item.Notes,
            item.Type, item.Count, item.NumOfEasy, item.NumOfNormal, item.NumOfHard,
            item.NumOfPractice, item.Method, item.AllowNotifyStudent,
            item.ExamAttempts.Count(attempt => !attempt.IsDeleted),
            item.ExamResults.Count(result => !result.IsDeleted)));

    public async Task<QuestionBankPageDto> GetQuestionPageAsync(QuestionBankQueryDto query, CancellationToken cancellationToken)
    {
        var source = dbContext.Questions.AsNoTracking();
        if (query.SuiteId.HasValue) source = source.Where(item => item.QuestionSuiteId == query.SuiteId);
        if (query.Level.HasValue) source = source.Where(item => item.Level == query.Level);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(item => item.QuestionText.Contains(search));
        }
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var total = await source.CountAsync(cancellationToken);
        var items = await source.OrderBy(item => item.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(item => new QuestionBankItemDto(item.Id, item.QuestionSuiteId,
                item.QuestionSuite.Name, item.QuestionSuite.SubjectId, item.QuestionText,
                item.Level, item.ImageUrl, item.QuestionAnswers.Count))
            .ToListAsync(cancellationToken);
        return new QuestionBankPageDto(items, pageNumber, pageSize, total);
    }

    public Task<QuestionBankDetailDto?> GetQuestionAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Questions.AsNoTracking().Where(item => item.Id == id)
            .Select(item => new QuestionBankDetailDto(item.Id, item.QuestionSuiteId,
                item.QuestionSuite.Name, item.QuestionSuite.SubjectId, item.QuestionText,
                item.Level, item.ImageUrl, item.QuestionAnswers.OrderBy(answer => answer.Id)
                    .Select(answer => new QuestionBankAnswerDto(answer.Id, answer.AnswerText,
                        answer.ImageUrl)).ToList()))
            .SingleOrDefaultAsync(cancellationToken);
}
