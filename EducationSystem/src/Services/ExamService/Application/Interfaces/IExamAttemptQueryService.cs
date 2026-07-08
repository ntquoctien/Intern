using ExamService.Application.DTOs.ExamAttempts;
using SharedKernel;

namespace ExamService.Application.Interfaces;

public interface IExamAttemptQueryService
{
    Task<PagedResult<ExamAttemptListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<ExamAttemptDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
