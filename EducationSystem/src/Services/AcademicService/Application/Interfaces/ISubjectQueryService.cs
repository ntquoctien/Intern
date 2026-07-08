using AcademicService.Application.DTOs.Subjects;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface ISubjectQueryService
{
    Task<PagedResult<SubjectListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<SubjectDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SubjectLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
