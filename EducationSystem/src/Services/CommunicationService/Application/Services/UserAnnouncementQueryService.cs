using CommunicationService.Application.DTOs.UserAnnouncements;
using CommunicationService.Application.Interfaces;
using CommunicationService.Infrastructure.Persistence;
using CommunicationService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace CommunicationService.Application.Services;

public sealed class UserAnnouncementQueryService : IUserAnnouncementQueryService
{
    private readonly CommunicationDbContext _dbContext;

    public UserAnnouncementQueryService(CommunicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<UserAnnouncementListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.UserAnnouncements.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
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
            source = source.Where(x => x.UserIds.Contains(search) || x.Message.Contains(search) || x.DeepLink.Contains(search) || x.DeepLinkParam.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserAnnouncementListItemDto
                    {
                        Id = x.Id,
                        Type = x.Type,
                        UserIds = x.UserIds,
                        Message = x.Message,
                        CreationDate = x.CreationDate,
                        Status = x.Status,
                        DeepLink = x.DeepLink,
                        DeepLinkParam = x.DeepLinkParam,
                        EntityObjectId = x.EntityObjectId,
                        EnforceRead = x.EnforceRead,
                        NotificationType = x.NotificationType,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<UserAnnouncementListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<UserAnnouncementDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserAnnouncements
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new UserAnnouncementDetailDto
                    {
                        Id = x.Id,
                        Type = x.Type,
                        UserIds = x.UserIds,
                        Message = x.Message,
                        CreationDate = x.CreationDate,
                        Status = x.Status,
                        DeepLink = x.DeepLink,
                        DeepLinkParam = x.DeepLinkParam,
                        EntityObjectId = x.EntityObjectId,
                        EnforceRead = x.EnforceRead,
                        NotificationType = x.NotificationType,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<UserAnnouncement> ApplySorting(IQueryable<UserAnnouncement> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "creationdate" => descending ? source.OrderByDescending(x => x.CreationDate) : source.OrderBy(x => x.CreationDate),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "type" => descending ? source.OrderByDescending(x => x.Type) : source.OrderBy(x => x.Type),
            "userids" => descending ? source.OrderByDescending(x => x.UserIds) : source.OrderBy(x => x.UserIds),
            "message" => descending ? source.OrderByDescending(x => x.Message) : source.OrderBy(x => x.Message),
            "status" => descending ? source.OrderByDescending(x => x.Status) : source.OrderBy(x => x.Status),
            "deeplink" => descending ? source.OrderByDescending(x => x.DeepLink) : source.OrderBy(x => x.DeepLink),
            "deeplinkparam" => descending ? source.OrderByDescending(x => x.DeepLinkParam) : source.OrderBy(x => x.DeepLinkParam),
            "entityobjectid" => descending ? source.OrderByDescending(x => x.EntityObjectId) : source.OrderBy(x => x.EntityObjectId),
            "enforceread" => descending ? source.OrderByDescending(x => x.EnforceRead) : source.OrderBy(x => x.EnforceRead),
            "notificationtype" => descending ? source.OrderByDescending(x => x.NotificationType) : source.OrderBy(x => x.NotificationType),
            "isdeleted" => descending ? source.OrderByDescending(x => x.IsDeleted) : source.OrderBy(x => x.IsDeleted),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
