using AcademicService.Application.DTOs.SemesterTuitions;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AcademicService.Application.Services;

public sealed class SemesterTuitionQueryService : ISemesterTuitionQueryService
{
    private readonly AcademicDbContext _dbContext;

    public SemesterTuitionQueryService(AcademicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SemesterTuitionListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.SemesterTuitions.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        if (filters?.StudentId is { } studentId)
        {
            source = source.Where(x => x.StudentId == studentId);
        }

        if (filters?.FromDate is { } fromDate)
        {
            source = source.Where(x => x.PaidDate >= fromDate);
        }

        if (filters?.ToDate is { } toDate)
        {
            source = source.Where(x => x.PaidDate <= toDate);
        }

        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SemesterTuitionListItemDto
                    {
                        Id = x.Id,
                        SemesterPlanId = x.SemesterPlanId,
                        StudentId = x.StudentId,
                        Amount = x.Amount,
                        PaidDate = x.PaidDate,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<SemesterTuitionListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<SemesterTuitionDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SemesterTuitions
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new SemesterTuitionDetailDto
                    {
                        Id = x.Id,
                        SemesterPlanId = x.SemesterPlanId,
                        StudentId = x.StudentId,
                        Amount = x.Amount,
                        PaidDate = x.PaidDate,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<SemesterTuition> ApplySorting(IQueryable<SemesterTuition> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "semesterplanid" => descending ? source.OrderByDescending(x => x.SemesterPlanId) : source.OrderBy(x => x.SemesterPlanId),
            "studentid" => descending ? source.OrderByDescending(x => x.StudentId) : source.OrderBy(x => x.StudentId),
            "amount" => descending ? source.OrderByDescending(x => x.Amount) : source.OrderBy(x => x.Amount),
            "paiddate" => descending ? source.OrderByDescending(x => x.PaidDate) : source.OrderBy(x => x.PaidDate),
            "isdeleted" => descending ? source.OrderByDescending(x => x.IsDeleted) : source.OrderBy(x => x.IsDeleted),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
