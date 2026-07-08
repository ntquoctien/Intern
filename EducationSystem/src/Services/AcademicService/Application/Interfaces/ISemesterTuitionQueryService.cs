using AcademicService.Application.DTOs.SemesterTuitions;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface ISemesterTuitionQueryService
{
    Task<PagedResult<SemesterTuitionListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<SemesterTuitionDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
