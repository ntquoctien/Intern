using ExamService.Application.DTOs.ExamResults;
using SharedKernel;

namespace ExamService.Application.Interfaces;

public interface IExamResultQueryService
{
    Task<PagedResult<ExamResultListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<ExamResultDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
