using ExamService.Application.DTOs.Questions;
using SharedKernel;

namespace ExamService.Application.Interfaces;

public interface IQuestionQueryService
{
    Task<PagedResult<QuestionListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<QuestionDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<QuestionLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
