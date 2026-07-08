using IdentityService.Application.DTOs.PasswordResets;
using IdentityService.Application.Interfaces;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace IdentityService.Application.Services;

public sealed class PasswordResetQueryService : IPasswordResetQueryService
{
    private readonly IdentityDbContext _dbContext;

    public PasswordResetQueryService(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<PasswordResetListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.PasswordResets.AsNoTracking();

        if (filters?.UserId is { } userId)
        {
            source = source.Where(x => x.UserId == userId);
        }

        if (filters?.FromDate is { } fromDate)
        {
            source = source.Where(x => x.CreationDate >= fromDate);
        }

        if (filters?.ToDate is { } toDate)
        {
            source = source.Where(x => x.CreationDate <= toDate);
        }

        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PasswordResetListItemDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        CreationDate = x.CreationDate,
                        IsDeactive = x.IsDeactive
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<PasswordResetListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<PasswordResetDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PasswordResets
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PasswordResetDetailDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        CreationDate = x.CreationDate,
                        IsDeactive = x.IsDeactive
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<PasswordReset> ApplySorting(IQueryable<PasswordReset> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "creationdate" => descending ? source.OrderByDescending(x => x.CreationDate) : source.OrderBy(x => x.CreationDate),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "userid" => descending ? source.OrderByDescending(x => x.UserId) : source.OrderBy(x => x.UserId),
            "isdeactive" => descending ? source.OrderByDescending(x => x.IsDeactive) : source.OrderBy(x => x.IsDeactive),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
