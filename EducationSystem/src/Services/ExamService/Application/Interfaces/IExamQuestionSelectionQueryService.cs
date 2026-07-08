using ExamService.Application.DTOs.ExamQuestionSelections;
using SharedKernel;

namespace ExamService.Application.Interfaces;

public interface IExamQuestionSelectionQueryService
{
    Task<PagedResult<ExamQuestionSelectionListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<ExamQuestionSelectionDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
