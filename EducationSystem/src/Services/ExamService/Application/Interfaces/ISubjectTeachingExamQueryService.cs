using ExamService.Application.DTOs.SubjectTeachingExams;
using SharedKernel;

namespace ExamService.Application.Interfaces;

public interface ISubjectTeachingExamQueryService
{
    Task<PagedResult<SubjectTeachingExamListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<SubjectTeachingExamDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<SubjectTeachingExamLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
