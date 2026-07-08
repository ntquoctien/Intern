using CommunicationService.Application.DTOs.UserAnnouncements;
using SharedKernel;

namespace CommunicationService.Application.Interfaces;

public interface IUserAnnouncementQueryService
{
    Task<PagedResult<UserAnnouncementListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<UserAnnouncementDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
