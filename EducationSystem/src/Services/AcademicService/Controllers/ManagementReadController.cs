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
    [HttpGet("plans/{id:guid}")] public async Task<ActionResult<ApiResponse<ManagementPlanDetailDto>>> Plan(Guid id, CancellationToken ct) { var item = await service.GetPlanAsync(id, ct); return item is null ? NotFound(ApiResponse<ManagementPlanDetailDto>.Fail("Semester plan not found.")) : Ok(ApiResponse<ManagementPlanDetailDto>.Ok(item)); }
    [HttpGet("teachers")] public async Task<ApiResponse<IReadOnlyList<TeacherSummaryDto>>> Teachers(CancellationToken ct) => ApiResponse<IReadOnlyList<TeacherSummaryDto>>.Ok(await service.GetTeachersAsync(ct));
    [HttpGet("teachers/{id:guid}")] public async Task<ActionResult<ApiResponse<TeacherDetailDto>>> Teacher(Guid id, CancellationToken ct) { var item = await service.GetTeacherAsync(id, ct); return item is null ? NotFound(ApiResponse<TeacherDetailDto>.Fail("Teacher not found.")) : Ok(ApiResponse<TeacherDetailDto>.Ok(item)); }
    [HttpGet("plans")] public async Task<ApiResponse<IReadOnlyList<ManagementPlanSummaryDto>>> Plans(CancellationToken ct) => ApiResponse<IReadOnlyList<ManagementPlanSummaryDto>>.Ok(await service.GetPlansAsync(ct));
    [HttpGet("classes")] public async Task<ApiResponse<IReadOnlyList<SubjectTeachingSummaryDto>>> Classes(CancellationToken ct) => ApiResponse<IReadOnlyList<SubjectTeachingSummaryDto>>.Ok(await service.GetClassesAsync(ct));
    [HttpGet("classes/{id:guid}")] public async Task<ActionResult<ApiResponse<SubjectTeachingDetailDto>>> Class(Guid id, CancellationToken ct) { var item = await service.GetClassAsync(id, ct); return item is null ? NotFound(ApiResponse<SubjectTeachingDetailDto>.Fail("Class not found.")) : Ok(ApiResponse<SubjectTeachingDetailDto>.Ok(item)); }
    [HttpGet("teaching-assignments")] public async Task<ApiResponse<IReadOnlyList<TeachingAssignmentDto>>> TeachingAssignments(CancellationToken ct) => ApiResponse<IReadOnlyList<TeachingAssignmentDto>>.Ok(await service.GetTeachingAssignmentsAsync(ct));
    [HttpGet("teaching-assignments/{teacherFacultyId:guid}")] public async Task<ActionResult<ApiResponse<TeacherAssignmentsDetailDto>>> TeacherAssignments(Guid teacherFacultyId, CancellationToken ct) { var item = await service.GetTeacherAssignmentsAsync(teacherFacultyId, ct); return item is null ? NotFound(ApiResponse<TeacherAssignmentsDetailDto>.Fail("Teacher assignments not found.")) : Ok(ApiResponse<TeacherAssignmentsDetailDto>.Ok(item)); }
    [HttpGet("rooms")] public async Task<ApiResponse<IReadOnlyList<ManagementRoomDto>>> Rooms(CancellationToken ct) => ApiResponse<IReadOnlyList<ManagementRoomDto>>.Ok(await service.GetRoomsAsync(ct));
    [HttpGet("teaching-schedule")] public async Task<ApiResponse<IReadOnlyList<ManagementTeachingScheduleDto>>> TeachingSchedule(CancellationToken ct) => ApiResponse<IReadOnlyList<ManagementTeachingScheduleDto>>.Ok(await service.GetTeachingScheduleAsync(ct));
    [HttpGet("subjects")] public async Task<ApiResponse<IReadOnlyList<ManagementSubjectSummaryDto>>> Subjects(CancellationToken ct) => ApiResponse<IReadOnlyList<ManagementSubjectSummaryDto>>.Ok(await service.GetSubjectsAsync(ct));
    [HttpGet("subjects/{id:guid}")] public async Task<ActionResult<ApiResponse<ManagementSubjectDetailDto>>> Subject(Guid id, CancellationToken ct) { var item = await service.GetSubjectAsync(id, ct); return item is null ? NotFound(ApiResponse<ManagementSubjectDetailDto>.Fail("Subject not found.")) : Ok(ApiResponse<ManagementSubjectDetailDto>.Ok(item)); }
    [HttpGet("schedule")] public async Task<ApiResponse<IReadOnlyList<ManagementScheduleDto>>> Schedule(CancellationToken ct) => ApiResponse<IReadOnlyList<ManagementScheduleDto>>.Ok(await service.GetScheduleAsync(ct));
    [HttpGet("attendance")] public async Task<ApiResponse<IReadOnlyList<ManagementAttendanceDto>>> Attendance(CancellationToken ct) => ApiResponse<IReadOnlyList<ManagementAttendanceDto>>.Ok(await service.GetAttendanceAsync(ct));
    [HttpGet("attendance-page")] public async Task<ApiResponse<AttendanceManagementPageDto>> AttendancePage([FromQuery] ManagementAttendanceQueryDto query, CancellationToken ct) => ApiResponse<AttendanceManagementPageDto>.Ok(await service.GetAttendancePageAsync(query, ct));
    [HttpGet("attendance-page/{id:guid}")] public async Task<ActionResult<ApiResponse<AttendanceManagementItemDto>>> AttendanceDetail(Guid id, CancellationToken ct) { var item = await service.GetAttendanceDetailAsync(id, ct); return item is null ? NotFound(ApiResponse<AttendanceManagementItemDto>.Fail("Attendance not found.")) : Ok(ApiResponse<AttendanceManagementItemDto>.Ok(item)); }
    [HttpGet("student-evaluations-page")] public async Task<ApiResponse<StudentEvaluationManagementPageDto>> StudentEvaluations([FromQuery] StudentEvaluationManagementQueryDto query, CancellationToken ct) => ApiResponse<StudentEvaluationManagementPageDto>.Ok(await service.GetStudentEvaluationPageAsync(query, ct));
    [HttpGet("student-evaluations-page/{id:guid}")] public async Task<ActionResult<ApiResponse<StudentEvaluationManagementItemDto>>> StudentEvaluation(Guid id, CancellationToken ct) { var item = await service.GetStudentEvaluationAsync(id, ct); return item is null ? NotFound(ApiResponse<StudentEvaluationManagementItemDto>.Fail("Evaluation not found.")) : Ok(ApiResponse<StudentEvaluationManagementItemDto>.Ok(item)); }
}
