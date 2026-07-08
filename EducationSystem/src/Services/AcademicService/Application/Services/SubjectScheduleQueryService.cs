using AcademicService.Application.DTOs.SubjectSchedules;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AcademicService.Application.Services;

public sealed class SubjectScheduleQueryService : ISubjectScheduleQueryService
{
    private readonly AcademicDbContext _dbContext;

    public SubjectScheduleQueryService(AcademicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SubjectScheduleListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.SubjectSchedules.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        if (filters?.SubjectTeachingId is { } subjectTeachingId)
        {
            source = source.Where(x => x.SubjectTeachingId == subjectTeachingId);
        }

        if (filters?.FromDate is { } fromDate)
        {
            source = source.Where(x => x.StartDateTime >= fromDate);
        }

        if (filters?.ToDate is { } toDate)
        {
            source = source.Where(x => x.StartDateTime <= toDate);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Note.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SubjectScheduleListItemDto
                    {
                        Id = x.Id,
                        SubjectTeachingId = x.SubjectTeachingId,
                        RoomId = x.RoomId,
                        TeacherId = x.TeacherId,
                        StartDateTime = x.StartDateTime,
                        EndDateTime = x.EndDateTime,
                        ScheduleType = x.ScheduleType,
                        Note = x.Note,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<SubjectScheduleListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<SubjectScheduleDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SubjectSchedules
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new SubjectScheduleDetailDto
                    {
                        Id = x.Id,
                        SubjectTeachingId = x.SubjectTeachingId,
                        RoomId = x.RoomId,
                        TeacherId = x.TeacherId,
                        StartDateTime = x.StartDateTime,
                        EndDateTime = x.EndDateTime,
                        ScheduleType = x.ScheduleType,
                        Note = x.Note,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<SubjectSchedule> ApplySorting(IQueryable<SubjectSchedule> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "startdatetime" => descending ? source.OrderByDescending(x => x.StartDateTime) : source.OrderBy(x => x.StartDateTime),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "subjectteachingid" => descending ? source.OrderByDescending(x => x.SubjectTeachingId) : source.OrderBy(x => x.SubjectTeachingId),
            "roomid" => descending ? source.OrderByDescending(x => x.RoomId) : source.OrderBy(x => x.RoomId),
            "teacherid" => descending ? source.OrderByDescending(x => x.TeacherId) : source.OrderBy(x => x.TeacherId),
            "enddatetime" => descending ? source.OrderByDescending(x => x.EndDateTime) : source.OrderBy(x => x.EndDateTime),
            "scheduletype" => descending ? source.OrderByDescending(x => x.ScheduleType) : source.OrderBy(x => x.ScheduleType),
            "note" => descending ? source.OrderByDescending(x => x.Note) : source.OrderBy(x => x.Note),
            "isdeleted" => descending ? source.OrderByDescending(x => x.IsDeleted) : source.OrderBy(x => x.IsDeleted),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
