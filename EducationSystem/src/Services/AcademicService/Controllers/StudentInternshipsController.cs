using System.Security.Cryptography;
using System.Text;
using AcademicService.Application.DTOs.Internships;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
public sealed class StudentInternshipsController(
    IStudentInternshipService internshipService,
    ICurrentStudentAccessor currentStudentAccessor,
    IConfiguration configuration,
    IHostEnvironment hostEnvironment) : ControllerBase
{
    [HttpPost("api/academic/internships/sync-approved")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<int>>> SyncApproved(
        [FromBody] SyncApprovedInternshipDto request,
        CancellationToken cancellationToken)
    {
        if (!HasValidInternalApiKey())
        {
            return Unauthorized(ApiResponse<int>.Fail(
                "INVALID_INTERNAL_API_KEY",
                "A valid internal service credential is required."));
        }

        try
        {
            var internshipId = await internshipService.SyncApprovedAsync(
                request,
                cancellationToken);
            return internshipId.HasValue
                ? Ok(ApiResponse<int>.Ok(internshipId.Value, "Approved internship synchronized."))
                : NotFound(ApiResponse<int>.Fail("STUDENT_NOT_FOUND", "Student was not found."));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ApiResponse<int>.Fail(
                "INVALID_INTERNSHIP_DATA",
                exception.Message));
        }
        catch (Exception exception)
        {
            var message = hostEnvironment.IsDevelopment()
                ? exception.GetBaseException().Message
                : "The approved internship could not be synchronized.";
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<int>.Fail("INTERNSHIP_SYNC_FAILED", message));
        }
    }

    [HttpGet("api/academic/students/{studentId:guid}/internships")]
    [Authorize(Policy = StudentClaimTypes.StudentPolicy)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentInternshipDto>>>> GetByStudent(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        if (currentStudentAccessor.GetRequiredStudent().StudentId != studentId)
        {
            return NotFound(ApiResponse<IReadOnlyList<StudentInternshipDto>>.Fail(
                StudentErrorCodes.OwnedResourceNotFound,
                "Internship history was not found."));
        }

        var result = await internshipService.GetByStudentAsync(studentId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<StudentInternshipDto>>.Ok(result));
    }

    [HttpGet("api/academic/internal/students/{studentId:guid}/internship-verification-profile")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<StudentInternshipVerificationProfileDto>>> GetVerificationProfile(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        if (!HasValidInternalApiKey())
        {
            return Unauthorized(ApiResponse<StudentInternshipVerificationProfileDto>.Fail(
                "INVALID_INTERNAL_API_KEY",
                "A valid internal service credential is required."));
        }

        var result = await internshipService.GetVerificationProfileAsync(
            studentId,
            cancellationToken);
        return result is null
            ? NotFound(ApiResponse<StudentInternshipVerificationProfileDto>.Fail(
                "STUDENT_NOT_FOUND",
                "Student verification profile was not found."))
            : Ok(ApiResponse<StudentInternshipVerificationProfileDto>.Ok(result));
    }

    private bool HasValidInternalApiKey()
    {
        var configuredKey = configuration["InternalApi:Key"];
        var suppliedKey = Request.Headers["X-Internal-Api-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(configuredKey) || string.IsNullOrWhiteSpace(suppliedKey))
        {
            return false;
        }

        var expected = Encoding.UTF8.GetBytes(configuredKey);
        var actual = Encoding.UTF8.GetBytes(suppliedKey);
        return expected.Length == actual.Length &&
            CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
