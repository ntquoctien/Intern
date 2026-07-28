using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using CommunicationService.Application.DTOs.InternshipVerification;
using CommunicationService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace CommunicationService.Controllers;

[ApiController]
[Route("api/communication/form-requests")]
public sealed class InternshipFormController(
    IInternshipVerificationService internshipService,
    ICurrentStudentAccessor currentStudentAccessor,
    IHostEnvironment hostEnvironment) : ControllerBase
{
    [HttpPost("submit-internship-request")]
    [Authorize(Policy = StudentClaimTypes.StudentPolicy)]
    public async Task<ActionResult<ApiResponse<InternshipRequestCreatedDto>>> SubmitInternshipRequest(
        [FromBody] SubmitInternshipRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var student = currentStudentAccessor.GetRequiredStudent();
            var result = await internshipService.SubmitAsync(
                student.StudentId,
                request,
                cancellationToken);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<InternshipRequestCreatedDto>.Ok(result, "Internship request submitted."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<InternshipRequestCreatedDto>.Fail(
                "INVALID_INTERNSHIP_DATES",
                exception.Message));
        }
    }

    [HttpPost("verify-by-employer")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<InternshipVerificationResultDto>>> VerifyByEmployer(
        [FromBody] VerifyInternshipByEmployerDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await internshipService.VerifyByEmployerAsync(request, cancellationToken);
            return result is null
                ? NotFound(ApiResponse<InternshipVerificationResultDto>.Fail(
                    "INVALID_OR_USED_EMPLOYER_TOKEN",
                    "The verification token is invalid or has already been used."))
                : Ok(ApiResponse<InternshipVerificationResultDto>.Ok(
                    result,
                    "Employer verification recorded."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<InternshipVerificationResultDto>.Fail(
                "INVALID_EMPLOYER_SCORE",
                exception.Message));
        }
    }

    [HttpGet("internship-verification-context")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<InternshipVerificationContextDto>>> GetVerificationContext(
        [FromQuery] Guid token,
        CancellationToken cancellationToken)
    {
        if (token == Guid.Empty)
        {
            return BadRequest(ApiResponse<InternshipVerificationContextDto>.Fail(
                "INVALID_EMPLOYER_TOKEN",
                "A valid verification token is required."));
        }

        var result = await internshipService.GetVerificationContextAsync(
            token,
            cancellationToken);
        return result is null
            ? NotFound(ApiResponse<InternshipVerificationContextDto>.Fail(
                "INVALID_OR_USED_EMPLOYER_TOKEN",
                "The verification token is invalid or has already been used."))
            : Ok(ApiResponse<InternshipVerificationContextDto>.Ok(result));
    }

    [HttpPost("admin-approve/{requestId:guid}")]
    public async Task<ActionResult<ApiResponse<InternshipApprovalResultDto>>> AdminApprove(
        Guid requestId,
        CancellationToken cancellationToken)
    {
        var isDevelopmentLoopback = hostEnvironment.IsDevelopment() &&
            HttpContext.Connection.RemoteIpAddress is { } remoteAddress &&
            IPAddress.IsLoopback(remoteAddress);
        var isAdministrator = User.Identity?.IsAuthenticated == true &&
            User.HasClaim("role", "Admin");
        if (!isAdministrator && !isDevelopmentLoopback)
        {
            return Unauthorized(ApiResponse<InternshipApprovalResultDto>.Fail(
                "ADMIN_AUTHENTICATION_REQUIRED",
                "An authenticated administrator is required."));
        }

        try
        {
            var administratorId = Guid.TryParse(
                User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                out var parsedId)
                ? parsedId
                : (Guid?)null;
            var administratorName =
                User.FindFirstValue("name") ?? User.Identity?.Name ?? "Administrator";
            var result = await internshipService.ApproveAsync(
                requestId,
                administratorId,
                administratorName,
                cancellationToken);

            return result is null
                ? NotFound(ApiResponse<InternshipApprovalResultDto>.Fail(
                    "FORM_REQUEST_NOT_FOUND",
                    "The internship request was not found."))
                : Ok(ApiResponse<InternshipApprovalResultDto>.Ok(
                    result,
                    "Internship approved by school."));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(ApiResponse<InternshipApprovalResultDto>.Fail(
                "EMPLOYER_VERIFICATION_REQUIRED",
                exception.Message));
        }
        catch (HttpRequestException exception)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                ApiResponse<InternshipApprovalResultDto>.Fail(
                    "ACADEMIC_SYNC_FAILED",
                    $"AcademicService could not synchronize the internship: {exception.Message}"));
        }
    }
}
