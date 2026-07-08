using AcademicService.Application.DTOs.SubjectSchedules;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface ISubjectScheduleQueryService
{
    Task<PagedResult<SubjectScheduleListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<SubjectScheduleDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
