using IdentityService.Application.DTOs.UserDevices;
using IdentityService.Application.Interfaces;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace IdentityService.Application.Services;

public sealed class UserDeviceQueryService : IUserDeviceQueryService
{
    private readonly IdentityDbContext _dbContext;

    public UserDeviceQueryService(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<UserDeviceListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.UserDevices.AsNoTracking();

        if (filters?.UserId is { } userId)
        {
            source = source.Where(x => x.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Identifier.Contains(search) || x.PushId.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserDeviceListItemDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        UserRole = x.UserRole,
                        DeviceType = x.DeviceType,
                        Identifier = x.Identifier,
                        PushId = x.PushId
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<UserDeviceListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<UserDeviceDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserDevices
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UserDeviceDetailDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        UserRole = x.UserRole,
                        DeviceType = x.DeviceType,
                        Identifier = x.Identifier,
                        PushId = x.PushId
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<UserDevice> ApplySorting(IQueryable<UserDevice> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "userid" => descending ? source.OrderByDescending(x => x.UserId) : source.OrderBy(x => x.UserId),
            "userrole" => descending ? source.OrderByDescending(x => x.UserRole) : source.OrderBy(x => x.UserRole),
            "devicetype" => descending ? source.OrderByDescending(x => x.DeviceType) : source.OrderBy(x => x.DeviceType),
            "identifier" => descending ? source.OrderByDescending(x => x.Identifier) : source.OrderBy(x => x.Identifier),
            "pushid" => descending ? source.OrderByDescending(x => x.PushId) : source.OrderBy(x => x.PushId),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
