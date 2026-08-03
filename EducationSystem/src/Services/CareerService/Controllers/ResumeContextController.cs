using CareerService.Application;
using CareerService.Application.DTOs.Resume;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace CareerService.Controllers;

[ApiController]
[Route("api/career/resume")]
[Authorize(AuthenticationSchemes = "Bearer", Policy = StudentClaimTypes.StudentPolicy)]
public sealed class ResumeContextController(
    IResumeContextHydrationService hydrationService,
    ICurrentStudentAccessor currentStudentAccessor) : ControllerBase
{
    [HttpPost("prepare-context")]
    public async Task<ActionResult<ApiResponse<ResumePromptPayloadDto>>> PrepareContext(
        [FromBody] PrepareResumePayloadRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentStudent = currentStudentAccessor.GetRequiredStudent();
        if (request.StudentId != currentStudent.StudentId)
            return NotFound(ApiResponse<ResumePromptPayloadDto>.Fail(
                "RESUME_CONTEXT_NOT_FOUND",
                "Resume context was not found."));

        var context = await hydrationService.HydrateResumeContextAsync(
            request,
            currentStudent.StudentId,
            currentStudent.UserId,
            GetBearerToken(),
            cancellationToken);
        return Ok(ApiResponse<ResumePromptPayloadDto>.Ok(
            context,
            "Resume prompt context prepared."));
    }

    private string GetBearerToken()
    {
        var authorization = Request.Headers.Authorization.ToString();
        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization["Bearer ".Length..].Trim()
            : string.Empty;
    }
}
