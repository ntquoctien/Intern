using AcademicService.Application.DTOs.SubjectTeachings;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AcademicService.Application.Services;

public sealed class SubjectTeachingQueryService : ISubjectTeachingQueryService
{
    private readonly AcademicDbContext _dbContext;

    public SubjectTeachingQueryService(AcademicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SubjectTeachingListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.SubjectTeachings.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        if (filters?.FromDate is { } fromDate)
        {
            source = source.Where(x => x.StartDate >= fromDate);
        }

        if (filters?.ToDate is { } toDate)
        {
            source = source.Where(x => x.StartDate <= toDate);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Name.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SubjectTeachingListItemDto
                    {
                        Id = x.Id,
                        SubjectId = x.SubjectId,
                        Name = x.Name,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        TotalSessions = x.TotalSessions,
                        RoomIdDefault = x.RoomIdDefault,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<SubjectTeachingListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<SubjectTeachingDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SubjectTeachings
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new SubjectTeachingDetailDto
                    {
                        Id = x.Id,
                        SubjectId = x.SubjectId,
                        Name = x.Name,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        TotalSessions = x.TotalSessions,
                        RoomIdDefault = x.RoomIdDefault,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<SubjectTeachingLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        var source = _dbContext.SubjectTeachings.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        return await source
            .OrderBy(x => x.Id)
            .Select(x => new SubjectTeachingLookupDto
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<SubjectTeaching> ApplySorting(IQueryable<SubjectTeaching> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => descending ? source.OrderByDescending(x => x.Name) : source.OrderBy(x => x.Name),
            "startdate" => descending ? source.OrderByDescending(x => x.StartDate) : source.OrderBy(x => x.StartDate),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "subjectid" => descending ? source.OrderByDescending(x => x.SubjectId) : source.OrderBy(x => x.SubjectId),
            "enddate" => descending ? source.OrderByDescending(x => x.EndDate) : source.OrderBy(x => x.EndDate),
            "totalsessions" => descending ? source.OrderByDescending(x => x.TotalSessions) : source.OrderBy(x => x.TotalSessions),
            "roomiddefault" => descending ? source.OrderByDescending(x => x.RoomIdDefault) : source.OrderBy(x => x.RoomIdDefault),
            "isdeleted" => descending ? source.OrderByDescending(x => x.IsDeleted) : source.OrderBy(x => x.IsDeleted),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
