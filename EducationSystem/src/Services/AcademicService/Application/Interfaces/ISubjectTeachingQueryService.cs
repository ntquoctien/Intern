using AcademicService.Application.DTOs.SubjectTeachings;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface ISubjectTeachingQueryService
{
    Task<PagedResult<SubjectTeachingListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<SubjectTeachingDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SubjectTeachingLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
