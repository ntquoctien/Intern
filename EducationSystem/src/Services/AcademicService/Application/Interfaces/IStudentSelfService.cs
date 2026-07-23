using AcademicService.Application.DTOs.StudentAccess;

namespace AcademicService.Application.Interfaces;

public interface IStudentSelfService
{
    Task<StudentProfileDto?> GetProfileAsync(Guid studentId, string studentCode, CancellationToken cancellationToken);
    Task<StudentProgramDto?> GetProgramAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StudentSubjectDto>> GetSubjectsAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StudentScheduleItemDto>> GetScheduleAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StudentAttendanceDto>> GetAttendanceAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StudentEvaluationDto>> GetEvaluationsAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StudentDocumentDto>> GetDocumentsAsync(Guid studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<StudentTuitionDto>> GetTuitionsAsync(Guid studentId, CancellationToken cancellationToken);
}
