using AcademicService.Application.DTOs.Resume;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/academic/resume")]
public sealed class ResumeDataController(
    IResumeDataService resumeDataService,
    ICurrentStudentAccessor currentStudentAccessor) : ControllerBase
{
    [HttpGet("get-context-data/{studentId:guid}")]
    [Authorize(Policy = StudentClaimTypes.StudentPolicy)]
    public async Task<ActionResult<ApiResponse<ResumeContextDto>>> GetContextData(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        if (currentStudentAccessor.GetRequiredStudent().StudentId != studentId)
        {
            return NotFound(ApiResponse<ResumeContextDto>.Fail(
                StudentErrorCodes.OwnedResourceNotFound,
                "Resume context data was not found."));
        }

        var result = await resumeDataService.GetContextDataAsync(studentId, cancellationToken);
        return result is null
            ? NotFound(ApiResponse<ResumeContextDto>.Fail(
                "STUDENT_NOT_FOUND",
                "Student or identity profile was not found."))
            : Ok(ApiResponse<ResumeContextDto>.Ok(result, "Resume context data prepared."));
    }
}
