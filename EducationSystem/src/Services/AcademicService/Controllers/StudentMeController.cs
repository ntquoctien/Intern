using AcademicService.Application.DTOs.StudentAccess;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Authorize(Policy = StudentClaimTypes.StudentPolicy)]
[Route("api/student/me")]
public sealed class StudentMeController(
    IStudentSelfService studentService,
    ICurrentStudentAccessor currentStudentAccessor) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<StudentProfileDto>>> Profile(CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        var profile = await studentService.GetProfileAsync(current.StudentId, current.StudentCode, cancellationToken);
        return profile is null
            ? NotFound(ApiResponse<StudentProfileDto>.Fail(StudentErrorCodes.OwnedResourceNotFound, "Student profile was not found."))
            : Ok(ApiResponse<StudentProfileDto>.Ok(profile));
    }

    [HttpGet("program")]
    public async Task<ActionResult<ApiResponse<StudentProgramDto>>> Program(CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        var program = await studentService.GetProgramAsync(current.StudentId, cancellationToken);
        return program is null
            ? NotFound(ApiResponse<StudentProgramDto>.Fail(StudentErrorCodes.OwnedResourceNotFound, "Student study plan was not found."))
            : Ok(ApiResponse<StudentProgramDto>.Ok(program));
    }

    [HttpGet("subjects")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentSubjectDto>>>> Subjects(CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        return Ok(ApiResponse<IReadOnlyList<StudentSubjectDto>>.Ok(
            await studentService.GetSubjectsAsync(current.StudentId, cancellationToken)));
    }

    [HttpGet("schedule")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentScheduleItemDto>>>> Schedule(CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        return Ok(ApiResponse<IReadOnlyList<StudentScheduleItemDto>>.Ok(
            await studentService.GetScheduleAsync(current.StudentId, cancellationToken)));
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentAttendanceDto>>>> Attendance(CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        return Ok(ApiResponse<IReadOnlyList<StudentAttendanceDto>>.Ok(
            await studentService.GetAttendanceAsync(current.StudentId, cancellationToken)));
    }

    [HttpGet("evaluations")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentEvaluationDto>>>> Evaluations(CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        return Ok(ApiResponse<IReadOnlyList<StudentEvaluationDto>>.Ok(
            await studentService.GetEvaluationsAsync(current.StudentId, cancellationToken)));
    }

    [HttpGet("documents")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentDocumentDto>>>> Documents(CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        return Ok(ApiResponse<IReadOnlyList<StudentDocumentDto>>.Ok(
            await studentService.GetDocumentsAsync(current.StudentId, cancellationToken)));
    }

    [HttpGet("tuition")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentTuitionDto>>>> Tuition(CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        return Ok(ApiResponse<IReadOnlyList<StudentTuitionDto>>.Ok(
            await studentService.GetTuitionsAsync(current.StudentId, cancellationToken)));
    }
}
