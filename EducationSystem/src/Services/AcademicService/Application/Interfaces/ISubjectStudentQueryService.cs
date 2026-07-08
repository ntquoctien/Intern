using AcademicService.Application.DTOs.SubjectStudents;
using SharedKernel;

namespace AcademicService.Application.Interfaces;

public interface ISubjectStudentQueryService
{
    Task<PagedResult<SubjectStudentListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<SubjectStudentDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
