using ExamService.Application.DTOs.Management;

namespace ExamService.Application.Interfaces;

public interface IManagementExamReadService
{
    Task<RawExamResultSummaryDto> GetResultSummaryAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ManagementRawExamResultDto>> GetResultsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<QuestionSuiteSummaryDto>> GetSuitesAsync(CancellationToken cancellationToken);
    Task<QuestionSuiteDetailDto?> GetSuiteAsync(Guid id, CancellationToken cancellationToken);
}
