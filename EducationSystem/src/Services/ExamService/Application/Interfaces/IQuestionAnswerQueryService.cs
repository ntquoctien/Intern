using ExamService.Application.DTOs.QuestionAnswers;
using SharedKernel;

namespace ExamService.Application.Interfaces;

public interface IQuestionAnswerQueryService
{
    Task<PagedResult<QuestionAnswerListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<QuestionAnswerDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
