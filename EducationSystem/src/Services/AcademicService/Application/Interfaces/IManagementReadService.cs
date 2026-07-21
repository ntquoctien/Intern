using AcademicService.Application.DTOs.Management;

namespace AcademicService.Application.Interfaces;

public interface IManagementReadService
{
    Task<ManagementDashboardDto> GetDashboardAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ManagementStudentSummaryDto>> GetStudentsAsync(CancellationToken cancellationToken);
    Task<ManagementStudentSummaryDto?> GetStudentAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<TeacherSummaryDto>> GetTeachersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ManagementPlanSummaryDto>> GetPlansAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<SubjectTeachingSummaryDto>> GetClassesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ManagementScheduleDto>> GetScheduleAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ManagementAttendanceDto>> GetAttendanceAsync(CancellationToken cancellationToken);
}
