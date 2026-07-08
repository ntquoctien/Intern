using AcademicService.Application.DTOs.Students;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface IStudentQueryService
{
    Task<PagedResult<StudentListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<StudentDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<StudentLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
