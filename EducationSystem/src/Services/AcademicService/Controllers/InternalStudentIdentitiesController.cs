using AcademicService.Application.DTOs.StudentAccess;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/internal/student-identities")]
public sealed class InternalStudentIdentitiesController(IStudentIdentityResolver resolver) : ControllerBase
{
    [HttpGet("by-nickname")]
    public async Task<ActionResult<ApiResponse<StudentIdentityResolutionDto>>> ResolveByNickname(
        [FromQuery] string code,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return BadRequest(ApiResponse<StudentIdentityResolutionDto>.Fail(
                StudentErrorCodes.NotFound,
                "Student code is required."));
        }

        var resolution = await resolver.ResolveByNicknameAsync(code, cancellationToken);
        return Ok(ApiResponse<StudentIdentityResolutionDto>.Ok(resolution));
    }

    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<ApiResponse<StudentIdentityResolutionDto>>> ResolveByUserId(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var resolution = await resolver.ResolveByUserIdAsync(userId, cancellationToken);
        return Ok(ApiResponse<StudentIdentityResolutionDto>.Ok(resolution));
    }
}
