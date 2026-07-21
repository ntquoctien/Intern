using ExamService.Application.DTOs.StudentAccess;
using ExamService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace ExamService.Controllers;

[ApiController]
[Authorize(Policy = StudentClaimTypes.StudentPolicy)]
[Route("api/student/me/exam-results")]
public sealed class StudentExamResultsController(
    IStudentExamResultService resultService,
    ICurrentStudentAccessor currentStudentAccessor) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentExamResultDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        return Ok(ApiResponse<IReadOnlyList<StudentExamResultDto>>.Ok(
            await resultService.GetAllAsync(current.StudentId, cancellationToken)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StudentExamResultDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        var result = await resultService.GetByIdAsync(current.StudentId, id, cancellationToken);
        return result is null
            ? NotFound(ApiResponse<StudentExamResultDto>.Fail(StudentErrorCodes.OwnedResourceNotFound, "Exam result was not found."))
            : Ok(ApiResponse<StudentExamResultDto>.Ok(result));
    }
}
