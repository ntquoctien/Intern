using AcademicService.Application.DTOs.Management;

namespace AcademicService.Application.Interfaces;

public interface IManagementReadService
{
    Task<ManagementDashboardDto> GetDashboardAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ManagementStudentSummaryDto>> GetStudentsAsync(CancellationToken cancellationToken);
    Task<ManagementStudentSummaryDto?> GetStudentAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TeacherSummaryDto>> GetTeachersAsync(CancellationToken cancellationToken);
    Task<TeacherDetailDto?> GetTeacherAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ManagementPlanSummaryDto>> GetPlansAsync(CancellationToken cancellationToken);
    Task<ManagementPlanDetailDto?> GetPlanAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<SubjectTeachingSummaryDto>> GetClassesAsync(CancellationToken cancellationToken);
    Task<SubjectTeachingDetailDto?> GetClassAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TeachingAssignmentDto>> GetTeachingAssignmentsAsync(CancellationToken cancellationToken);
    Task<TeacherAssignmentsDetailDto?> GetTeacherAssignmentsAsync(Guid teacherFacultyId, CancellationToken cancellationToken);
    Task<IReadOnlyList<ManagementRoomDto>> GetRoomsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ManagementTeachingScheduleDto>> GetTeachingScheduleAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ManagementSubjectSummaryDto>> GetSubjectsAsync(CancellationToken cancellationToken);
    Task<ManagementSubjectDetailDto?> GetSubjectAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ManagementScheduleDto>> GetScheduleAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ManagementAttendanceDto>> GetAttendanceAsync(CancellationToken cancellationToken);
}
