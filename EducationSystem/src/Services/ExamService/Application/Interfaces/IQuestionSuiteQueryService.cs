using ExamService.Application.DTOs.QuestionSuites;
using SharedKernel;

namespace ExamService.Application.Interfaces;

public interface IQuestionSuiteQueryService
{
    Task<PagedResult<QuestionSuiteListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<QuestionSuiteDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<QuestionSuiteLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
