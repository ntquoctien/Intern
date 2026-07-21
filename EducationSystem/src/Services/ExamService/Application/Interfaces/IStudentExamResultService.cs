using ExamService.Application.DTOs.StudentAccess;

namespace ExamService.Application.Interfaces;

public interface IStudentExamResultService
{
    Task<IReadOnlyList<StudentExamResultDto>> GetAllAsync(Guid studentId, CancellationToken cancellationToken);
    Task<StudentExamResultDto?> GetByIdAsync(Guid studentId, Guid resultId, CancellationToken cancellationToken);
}
