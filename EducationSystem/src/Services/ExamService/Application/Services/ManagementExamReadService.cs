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
                suite.CreationTime, suite.Questions.Count))
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
}
