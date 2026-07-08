using IdentityService.Application.DTOs.AuditLogs;
using IdentityService.Application.Interfaces;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace IdentityService.Application.Services;

public sealed class AuditLogQueryService : IAuditLogQueryService
{
    private readonly IdentityDbContext _dbContext;

    public AuditLogQueryService(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AuditLogListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.AuditLogs.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
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

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Details.Contains(search) || x.RecordDesc.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditLogListItemDto
                    {
                        Id = x.Id,
                        Action = x.Action,
                        Details = x.Details,
                        RecordId = x.RecordId,
                        CreationDate = x.CreationDate,
                        UserId = x.UserId,
                        RecordEntity = x.RecordEntity,
                        RecordDesc = x.RecordDesc,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<AuditLogDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AuditLogs
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new AuditLogDetailDto
                    {
                        Id = x.Id,
                        Action = x.Action,
                        Details = x.Details,
                        RecordId = x.RecordId,
                        CreationDate = x.CreationDate,
                        UserId = x.UserId,
                        RecordEntity = x.RecordEntity,
                        RecordDesc = x.RecordDesc,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<AuditLog> ApplySorting(IQueryable<AuditLog> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "creationdate" => descending ? source.OrderByDescending(x => x.CreationDate) : source.OrderBy(x => x.CreationDate),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "action" => descending ? source.OrderByDescending(x => x.Action) : source.OrderBy(x => x.Action),
            "details" => descending ? source.OrderByDescending(x => x.Details) : source.OrderBy(x => x.Details),
            "recordid" => descending ? source.OrderByDescending(x => x.RecordId) : source.OrderBy(x => x.RecordId),
            "userid" => descending ? source.OrderByDescending(x => x.UserId) : source.OrderBy(x => x.UserId),
            "recordentity" => descending ? source.OrderByDescending(x => x.RecordEntity) : source.OrderBy(x => x.RecordEntity),
            "recorddesc" => descending ? source.OrderByDescending(x => x.RecordDesc) : source.OrderBy(x => x.RecordDesc),
            "isdeleted" => descending ? source.OrderByDescending(x => x.IsDeleted) : source.OrderBy(x => x.IsDeleted),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
