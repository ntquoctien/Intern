using ExamService.Application.DTOs.ExamQuestionAnswers;
using SharedKernel;

namespace ExamService.Application.Interfaces;

public interface IExamQuestionAnswerQueryService
{
    Task<PagedResult<ExamQuestionAnswerListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<ExamQuestionAnswerDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
