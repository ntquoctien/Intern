using System.IdentityModel.Tokens.Jwt;
using IdentityService.Application.StudentAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/auth/student")]
public sealed class StudentAuthController(
    IStudentLoginService loginService,
    ICurrentStudentAccessor currentStudentAccessor) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<StudentLoginResponse>>> Login(
        StudentLoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await loginService.LoginAsync(
            request.StudentCode ?? string.Empty,
            request.Credential ?? string.Empty,
            cancellationToken);
        if (result.Success)
        {
            return Ok(ApiResponse<StudentLoginResponse>.Ok(result.Response));
        }

        var response = ApiResponse<StudentLoginResponse>.Fail(result.ErrorCode!, result.ErrorMessage!);
        return result.ErrorCode switch
        {
            StudentErrorCodes.CodeAmbiguous => Conflict(response),
            StudentErrorCodes.IdentityServiceUnavailable => StatusCode(StatusCodes.Status503ServiceUnavailable, response),
            _ => Unauthorized(response)
        };
    }

    [Authorize(Policy = StudentClaimTypes.StudentPolicy)]
    [HttpGet("/api/student/me/session")]
    public async Task<ActionResult<ApiResponse<StudentSessionDto>>> Session(CancellationToken cancellationToken)
    {
        var currentStudent = currentStudentAccessor.GetRequiredStudent();
        var expiryClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        var expiresAt = long.TryParse(expiryClaim, out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : DateTimeOffset.UtcNow;
        var session = await loginService.GetSessionAsync(currentStudent, expiresAt, cancellationToken);
        return session is null
            ? Unauthorized(ApiResponse<StudentSessionDto>.Fail(
                StudentErrorCodes.AccountUnavailable,
                "The linked student account is unavailable."))
            : Ok(ApiResponse<StudentSessionDto>.Ok(session));
    }
}
