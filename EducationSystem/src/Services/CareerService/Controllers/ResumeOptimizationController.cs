using CareerService.Application;
using CareerService.Application.DTOs.Resume;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace CareerService.Controllers;

[ApiController]
[Route("api/career/resume")]
[Authorize(AuthenticationSchemes = "Bearer", Policy = StudentClaimTypes.StudentPolicy)]
public sealed class ResumeOptimizationController(
    IResumeContextHydrationService hydrationService,
    ILlmResumeGeneratorService generatorService,
    ICurrentStudentAccessor currentStudentAccessor) : ControllerBase
{
    [HttpPost("optimize")]
    public async Task<ActionResult<ApiResponse<OptimizedResumeResponseDto>>> OptimizeResume(
        [FromBody] PrepareResumePayloadRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentStudent = currentStudentAccessor.GetRequiredStudent();
        if (request.StudentId != currentStudent.StudentId)
            return NotFound(ApiResponse<OptimizedResumeResponseDto>.Fail(
                "RESUME_CONTEXT_NOT_FOUND",
                "Resume context was not found."));

        var context = await hydrationService.HydrateResumeContextAsync(
            request,
            currentStudent.StudentId,
            currentStudent.UserId,
            GetBearerToken(),
            cancellationToken);
        var result = await generatorService.GenerateOptimizedResumeAsync(
            context,
            cancellationToken);
        return Ok(ApiResponse<OptimizedResumeResponseDto>.Ok(
            result,
            "Vietnamese ATS resume generated."));
    }

    private string GetBearerToken()
    {
        var authorization = Request.Headers.Authorization.ToString();
        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization["Bearer ".Length..].Trim()
            : string.Empty;
    }
}
