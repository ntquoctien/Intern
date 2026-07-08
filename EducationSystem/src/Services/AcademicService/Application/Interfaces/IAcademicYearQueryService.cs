using AcademicService.Application.DTOs.AcademicYears;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface IAcademicYearQueryService
{
    Task<PagedResult<AcademicYearListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<AcademicYearDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<AcademicYearLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
