using AcademicService.Application.DTOs.StudentEvaluations;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface IStudentEvaluationQueryService
{
    Task<PagedResult<StudentEvaluationListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<StudentEvaluationDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
