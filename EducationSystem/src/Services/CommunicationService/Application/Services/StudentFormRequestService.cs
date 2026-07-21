using CommunicationService.Application.DTOs.StudentAccess;
using CommunicationService.Application.Interfaces;
using CommunicationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommunicationService.Application.Services;

public sealed class StudentFormRequestService(CommunicationDbContext dbContext) : IStudentFormRequestService
{
    public async Task<IReadOnlyList<StudentFormRequestDto>> GetAllAsync(
        Guid studentId,
        CancellationToken cancellationToken) =>
        await OwnedQuery(studentId)
            .OrderByDescending(request => request.CreationDate)
            .Select(Project())
            .ToListAsync(cancellationToken);

    public Task<StudentFormRequestDto?> GetByIdAsync(
        Guid studentId,
        Guid requestId,
        CancellationToken cancellationToken) =>
        OwnedQuery(studentId)
            .Where(request => request.Id == requestId)
            .Select(Project())
            .SingleOrDefaultAsync(cancellationToken);

    private IQueryable<Infrastructure.Persistence.Entities.FormRequest> OwnedQuery(Guid studentId) =>
        dbContext.FormRequests.AsNoTracking()
            .Where(request => request.StudentId == studentId && !request.IsDeleted);

    private static System.Linq.Expressions.Expression<Func<Infrastructure.Persistence.Entities.FormRequest, StudentFormRequestDto>> Project() =>
        request => new StudentFormRequestDto(
            request.Id,
            request.FormTemplateId,
            request.FormTemplate != null ? request.FormTemplate.Name : null,
            request.FormTemplate != null ? request.FormTemplate.DocumentUrl : null,
            request.CreationDate,
            request.UpdateDate,
            request.Status,
            request.ApprovalId,
            request.ApprovalName,
            request.Note);
}
