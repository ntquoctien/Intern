using AcademicService.Application.DTOs.Majors;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface IMajorQueryService
{
    Task<PagedResult<MajorListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<MajorDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<MajorLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
