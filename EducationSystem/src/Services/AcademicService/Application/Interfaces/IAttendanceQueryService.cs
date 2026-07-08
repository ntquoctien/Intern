using AcademicService.Application.DTOs.Attendances;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface IAttendanceQueryService
{
    Task<PagedResult<AttendanceListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<AttendanceDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
