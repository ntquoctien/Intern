using AcademicService.Application.DTOs.StudentAccess;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AcademicService.Application.Services;

public sealed class StudentIdentityResolver(AcademicDbContext dbContext) : IStudentIdentityResolver
{
    public Task<StudentIdentityResolutionDto> ResolveByNicknameAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim();
        return ResolveAsync(
            dbContext.Students.AsNoTracking().Where(student =>
                !student.IsDeleted && student.Nickname != null && student.Nickname == normalizedCode),
            cancellationToken);
    }

    public Task<StudentIdentityResolutionDto> ResolveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        ResolveAsync(
            dbContext.Students.AsNoTracking().Where(student => !student.IsDeleted && student.UserId == userId),
            cancellationToken);

    private static async Task<StudentIdentityResolutionDto> ResolveAsync(
        IQueryable<Infrastructure.Persistence.Entities.Student> query,
        CancellationToken cancellationToken)
    {
        var matches = await query
            .OrderBy(student => student.Id)
            .Take(2)
            .Select(student => new StudentIdentityCandidateDto(
                student.Id,
                student.UserId,
                student.Nickname ?? string.Empty,
                student.StudyStatus,
                student.IsGraduated,
                student.HasIssue))
            .ToListAsync(cancellationToken);

        return matches.Count == 1
            ? new StudentIdentityResolutionDto(1, matches[0])
            : new StudentIdentityResolutionDto(matches.Count, null);
    }
}
