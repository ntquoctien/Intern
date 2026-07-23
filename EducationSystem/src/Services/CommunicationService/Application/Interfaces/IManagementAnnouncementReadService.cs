using CommunicationService.Application.DTOs.Management;

namespace CommunicationService.Application.Interfaces;

public interface IManagementAnnouncementReadService
{
    Task<ManagementAnnouncementPageDto> GetPageAsync(ManagementAnnouncementQueryDto query, CancellationToken cancellationToken);
    Task<ManagementAnnouncementItemDto?> GetAsync(Guid id, CancellationToken cancellationToken);
}
