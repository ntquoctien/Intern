using AcademicService.Application.DTOs.Faculties;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface IFacultyQueryService
{
    Task<PagedResult<FacultyListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<FacultyDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<FacultyLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
