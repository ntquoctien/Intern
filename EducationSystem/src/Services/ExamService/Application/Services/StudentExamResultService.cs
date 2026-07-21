using ExamService.Application.DTOs.StudentAccess;
using ExamService.Application.Interfaces;
using ExamService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExamService.Application.Services;

public sealed class StudentExamResultService(ExamDbContext dbContext) : IStudentExamResultService
{
    public async Task<IReadOnlyList<StudentExamResultDto>> GetAllAsync(
        Guid studentId,
        CancellationToken cancellationToken) =>
        await OwnedQuery(studentId)
            .OrderByDescending(result => result.SubjectTeachingExam.StartDate)
            .ThenBy(result => result.Id)
            .Select(Project())
            .ToListAsync(cancellationToken);

    public Task<StudentExamResultDto?> GetByIdAsync(
        Guid studentId,
        Guid resultId,
        CancellationToken cancellationToken) =>
        OwnedQuery(studentId)
            .Where(result => result.Id == resultId)
            .Select(Project())
            .SingleOrDefaultAsync(cancellationToken);

    private IQueryable<Infrastructure.Persistence.Entities.ExamResult> OwnedQuery(Guid studentId) =>
        dbContext.ExamResults.AsNoTracking()
            .Where(result => result.StudentId == studentId &&
                !result.IsDeleted &&
                !result.SubjectTeachingExam.IsDeleted);

    private static System.Linq.Expressions.Expression<Func<Infrastructure.Persistence.Entities.ExamResult, StudentExamResultDto>> Project() =>
        result => new StudentExamResultDto(
            result.Id,
            result.SubjectTeachingExamId,
            result.SubjectTeachingExam.SubjectTeachingId,
            result.ExamAttemptId,
            result.SubjectTeachingExam.Name,
            result.SubjectTeachingExam.StartDate,
            result.SubjectTeachingExam.EndDate,
            result.SubjectTeachingExam.Type,
            result.ExamAttempt != null ? result.ExamAttempt.SubmitDate : null,
            result.Result,
            result.CombinedResult,
            result.ExamResultDesc,
            result.Notes);
}
