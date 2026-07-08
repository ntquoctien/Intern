using AcademicService.Application.DTOs.EvaluationCriterias;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface IEvaluationCriteriaQueryService
{
    Task<PagedResult<EvaluationCriteriaListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<EvaluationCriteriaDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<EvaluationCriteriaLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
