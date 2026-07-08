using AcademicService.Application.DTOs.Rooms;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface IRoomQueryService
{
    Task<PagedResult<RoomListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<RoomDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<RoomLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
