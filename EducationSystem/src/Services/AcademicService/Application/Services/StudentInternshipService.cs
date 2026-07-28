using System.Data;
using AcademicService.Application.DTOs.Internships;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using EducationSystem.Services.Academic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicService.Application.Services;

public sealed class StudentInternshipService(
    AcademicDbContext dbContext,
    IIdentityUserClient identityUserClient) : IStudentInternshipService
{
    public async Task<int?> SyncApprovedAsync(
        SyncApprovedInternshipDto request,
        CancellationToken cancellationToken)
    {
        if (request.FormRequestId == Guid.Empty)
        {
            throw new ArgumentException("FormRequestId is required.");
        }

        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
        {
            throw new ArgumentException("EndDate must be greater than or equal to StartDate.");
        }

        var studentExists = await dbContext.Students.AsNoTracking()
            .AnyAsync(
                student => student.Id == request.StudentId && !student.IsDeleted,
                cancellationToken);
        if (!studentExists)
        {
            return null;
        }

        if (!dbContext.Database.IsRelational())
        {
            return await FindOrCreateAsync(request, cancellationToken);
        }

        // Serialize retries for the same source request. This keeps the
        // find-or-create operation idempotent without relying on a filtered
        // unique index and its connection-level SQL Server SET requirements.
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            // Backward compatible with databases created by the first Phase 1
            // script, which used a filtered unique index on FormRequestID.
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                SET ANSI_NULLS ON;
                SET ANSI_PADDING ON;
                SET ANSI_WARNINGS ON;
                SET ARITHABORT ON;
                SET CONCAT_NULL_YIELDS_NULL ON;
                SET QUOTED_IDENTIFIER ON;
                SET NUMERIC_ROUNDABORT OFF;
                """,
                cancellationToken);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
            var internshipId = await FindOrCreateAsync(request, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return internshipId;
        }
        finally
        {
            await dbContext.Database.CloseConnectionAsync();
        }
    }

    private async Task<int> FindOrCreateAsync(
        SyncApprovedInternshipDto request,
        CancellationToken cancellationToken)
    {
        // Idempotent inter-service retry. Relational callers execute this inside
        // a serializable transaction to prevent concurrent duplicate inserts.
        var existingId = await dbContext.StudentInternships.AsNoTracking()
            .Where(item => item.FormRequestId == request.FormRequestId)
            .Select(item => (int?)item.InternshipId)
            .SingleOrDefaultAsync(cancellationToken);
        if (existingId.HasValue)
        {
            return existingId.Value;
        }

        var entity = new StudentInternship
        {
            StudentId = request.StudentId,
            CompanyName = request.CompanyName.Trim(),
            Position = request.Position.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TaskDescription = string.IsNullOrWhiteSpace(request.TaskDescription)
                ? null
                : request.TaskDescription.Trim(),
            FormRequestId = request.FormRequestId
        };

        dbContext.StudentInternships.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.InternshipId;
    }

    public async Task<IReadOnlyList<StudentInternshipDto>> GetByStudentAsync(
        Guid studentId,
        CancellationToken cancellationToken) =>
        await dbContext.StudentInternships.AsNoTracking()
            .Where(item => item.StudentId == studentId)
            .OrderByDescending(item => item.StartDate)
            .ThenByDescending(item => item.InternshipId)
            .Select(item => new StudentInternshipDto(
                item.InternshipId,
                item.StudentId,
                item.CompanyName,
                item.Position,
                item.StartDate,
                item.EndDate,
                item.TaskDescription,
                item.FormRequestId))
            .ToListAsync(cancellationToken);

    public async Task<StudentInternshipVerificationProfileDto?> GetVerificationProfileAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students.AsNoTracking()
            .Where(item => item.Id == studentId && !item.IsDeleted)
            .Select(item => new
            {
                item.Id,
                item.UserId,
                StudentCode = item.Nickname ?? string.Empty
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (student is null)
        {
            return null;
        }

        var identity = await identityUserClient.GetAsync(student.UserId, cancellationToken);
        if (identity is null || !identity.IsActive)
        {
            return null;
        }

        return new StudentInternshipVerificationProfileDto(
            student.Id,
            student.StudentCode,
            identity.FullName);
    }
}
