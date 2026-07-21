using AcademicService.Application.DTOs.Management;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/management")]
public sealed class ManagementReadController(IManagementReadService service) : ControllerBase
{
    [HttpGet("dashboard")] public async Task<ApiResponse<ManagementDashboardDto>> Dashboard(CancellationToken ct) => ApiResponse<ManagementDashboardDto>.Ok(await service.GetDashboardAsync(ct));
    [HttpGet("students")] public async Task<ApiResponse<IReadOnlyList<ManagementStudentSummaryDto>>> Students(CancellationToken ct) => ApiResponse<IReadOnlyList<ManagementStudentSummaryDto>>.Ok(await service.GetStudentsAsync(ct));
    [HttpGet("students/{id:guid}")] public async Task<ActionResult<ApiResponse<ManagementStudentSummaryDto>>> Student(Guid id, CancellationToken ct) { var item = await service.GetStudentAsync(id, ct); return item is null ? NotFound(ApiResponse<ManagementStudentSummaryDto>.Fail("Student not found.")) : Ok(ApiResponse<ManagementStudentSummaryDto>.Ok(item)); }
    [HttpGet("teachers")] public async Task<ApiResponse<IReadOnlyList<TeacherSummaryDto>>> Teachers(CancellationToken ct) => ApiResponse<IReadOnlyList<TeacherSummaryDto>>.Ok(await service.GetTeachersAsync(ct));
    [HttpGet("plans")] public async Task<ApiResponse<IReadOnlyList<ManagementPlanSummaryDto>>> Plans(CancellationToken ct) => ApiResponse<IReadOnlyList<ManagementPlanSummaryDto>>.Ok(await service.GetPlansAsync(ct));
    [HttpGet("classes")] public async Task<ApiResponse<IReadOnlyList<SubjectTeachingSummaryDto>>> Classes(CancellationToken ct) => ApiResponse<IReadOnlyList<SubjectTeachingSummaryDto>>.Ok(await service.GetClassesAsync(ct));
    [HttpGet("schedule")] public async Task<ApiResponse<IReadOnlyList<ManagementScheduleDto>>> Schedule(CancellationToken ct) => ApiResponse<IReadOnlyList<ManagementScheduleDto>>.Ok(await service.GetScheduleAsync(ct));
    [HttpGet("attendance")] public async Task<ApiResponse<IReadOnlyList<ManagementAttendanceDto>>> Attendance(CancellationToken ct) => ApiResponse<IReadOnlyList<ManagementAttendanceDto>>.Ok(await service.GetAttendanceAsync(ct));
}
