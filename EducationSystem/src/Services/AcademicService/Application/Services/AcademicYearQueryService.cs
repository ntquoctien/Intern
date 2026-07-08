using AcademicService.Application.DTOs.AcademicYears;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AcademicService.Application.Services;

public sealed class AcademicYearQueryService : IAcademicYearQueryService
{
    private readonly AcademicDbContext _dbContext;

    public AcademicYearQueryService(AcademicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AcademicYearListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.AcademicYears.AsNoTracking();
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
            .Select(x => new AcademicYearListItemDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Year = x.Year,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<AcademicYearListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<AcademicYearDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AcademicYears
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new AcademicYearDetailDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Year = x.Year,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<AcademicYearLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        var source = _dbContext.AcademicYears.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        return await source
            .OrderBy(x => x.Id)
            .Select(x => new AcademicYearLookupDto
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<AcademicYear> ApplySorting(IQueryable<AcademicYear> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => descending ? source.OrderByDescending(x => x.Name) : source.OrderBy(x => x.Name),
            "startdate" => descending ? source.OrderByDescending(x => x.StartDate) : source.OrderBy(x => x.StartDate),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "year" => descending ? source.OrderByDescending(x => x.Year) : source.OrderBy(x => x.Year),
            "enddate" => descending ? source.OrderByDescending(x => x.EndDate) : source.OrderBy(x => x.EndDate),
            "isdeleted" => descending ? source.OrderByDescending(x => x.IsDeleted) : source.OrderBy(x => x.IsDeleted),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
