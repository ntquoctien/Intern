using AcademicService.Application.DTOs.Attendances;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AcademicService.Application.Services;

public sealed class AttendanceQueryService : IAttendanceQueryService
{
    private readonly AcademicDbContext _dbContext;

    public AttendanceQueryService(AcademicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AttendanceListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.Attendances.AsNoTracking();

        if (filters?.StudentId is { } studentId)
        {
            source = source.Where(x => x.StudentId == studentId);
        }

        if (filters?.SubjectScheduleId is { } subjectScheduleId)
        {
            source = source.Where(x => x.SubjectScheduleId == subjectScheduleId);
        }

        if (filters?.Status is { } status)
        {
            source = source.Where(x => x.Status == status);
        }

        if (filters?.FromDate is { } fromDate)
        {
            source = source.Where(x => x.CreationDate >= fromDate);
        }

        if (filters?.ToDate is { } toDate)
        {
            source = source.Where(x => x.CreationDate <= toDate);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Notes.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AttendanceListItemDto
                    {
                        Id = x.Id,
                        SubjectScheduleId = x.SubjectScheduleId,
                        StudentId = x.StudentId,
                        CreatedById = x.CreatedById,
                        Status = x.Status,
                        Notes = x.Notes,
                        CreationDate = x.CreationDate,
                        IsFirstTypeWarning = x.IsFirstTypeWarning,
                        IsSecondTypeWarning = x.IsSecondTypeWarning
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<AttendanceListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<AttendanceDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Attendances
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AttendanceDetailDto
                    {
                        Id = x.Id,
                        SubjectScheduleId = x.SubjectScheduleId,
                        StudentId = x.StudentId,
                        CreatedById = x.CreatedById,
                        Status = x.Status,
                        Notes = x.Notes,
                        CreationDate = x.CreationDate,
                        IsFirstTypeWarning = x.IsFirstTypeWarning,
                        IsSecondTypeWarning = x.IsSecondTypeWarning
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<Attendance> ApplySorting(IQueryable<Attendance> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "creationdate" => descending ? source.OrderByDescending(x => x.CreationDate) : source.OrderBy(x => x.CreationDate),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "subjectscheduleid" => descending ? source.OrderByDescending(x => x.SubjectScheduleId) : source.OrderBy(x => x.SubjectScheduleId),
            "studentid" => descending ? source.OrderByDescending(x => x.StudentId) : source.OrderBy(x => x.StudentId),
            "createdbyid" => descending ? source.OrderByDescending(x => x.CreatedById) : source.OrderBy(x => x.CreatedById),
            "status" => descending ? source.OrderByDescending(x => x.Status) : source.OrderBy(x => x.Status),
            "notes" => descending ? source.OrderByDescending(x => x.Notes) : source.OrderBy(x => x.Notes),
            "isfirsttypewarning" => descending ? source.OrderByDescending(x => x.IsFirstTypeWarning) : source.OrderBy(x => x.IsFirstTypeWarning),
            "issecondtypewarning" => descending ? source.OrderByDescending(x => x.IsSecondTypeWarning) : source.OrderBy(x => x.IsSecondTypeWarning),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
