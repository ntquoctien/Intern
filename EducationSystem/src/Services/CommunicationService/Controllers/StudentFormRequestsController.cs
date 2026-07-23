using CommunicationService.Application.DTOs.StudentAccess;
using CommunicationService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace CommunicationService.Controllers;

[ApiController]
[Authorize(Policy = StudentClaimTypes.StudentPolicy)]
[Route("api/student/me/form-requests")]
public sealed class StudentFormRequestsController(
    IStudentFormRequestService requestService,
    ICurrentStudentAccessor currentStudentAccessor) : ControllerBase
{
    [HttpGet("templates")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentFormTemplateDto>>>> GetTemplates(
        CancellationToken cancellationToken) =>
        Ok(ApiResponse<IReadOnlyList<StudentFormTemplateDto>>.Ok(
            await requestService.GetTemplatesAsync(cancellationToken)));

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentFormRequestDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        return Ok(ApiResponse<IReadOnlyList<StudentFormRequestDto>>.Ok(
            await requestService.GetAllAsync(current.StudentId, cancellationToken)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StudentFormRequestDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        var request = await requestService.GetByIdAsync(current.StudentId, id, cancellationToken);
        return request is null
            ? NotFound(ApiResponse<StudentFormRequestDto>.Fail(StudentErrorCodes.OwnedResourceNotFound, "Form request was not found."))
            : Ok(ApiResponse<StudentFormRequestDto>.Ok(request));
    }
}
