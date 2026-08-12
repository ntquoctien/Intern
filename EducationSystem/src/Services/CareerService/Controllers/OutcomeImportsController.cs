using System.Security.Claims;
using CareerService.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerService.Controllers;

[ApiController]
[Route("api/management")]
[Authorize(Policy = "SystemAdministrator")]
public sealed class OutcomeImportsController(
    OutcomeImportService service) : ControllerBase
{
    [HttpGet("curriculum-versions")]
    public Task<IReadOnlyList<CurriculumOptionDto>> Curricula(CancellationToken cancellationToken) =>
        service.GetCurriculaAsync(cancellationToken);

    [HttpPost("curriculum-versions")]
    public async Task<ActionResult<CurriculumOptionDto>> CreateCurriculum(
        [FromBody] CreateCurriculumRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.CreateCurriculumAsync(request, cancellationToken);
        return Created($"/api/management/curriculum-versions/{result.Id}", result);
    }

    [HttpGet("outcome-imports")]
    public Task<PagedResponse<OutcomeImportListItemDto>> List(
        [FromQuery] string? status,
        [FromQuery] string? majorCode,
        [FromQuery] long? curriculumVersionId,
        [FromQuery] string? uploadedBy,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        service.ListAsync(
            status, majorCode, curriculumVersionId, uploadedBy,
            fromDate, toDate, keyword, page, pageSize, cancellationToken);

    [HttpPost("outcome-imports")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024 + 1024)]
    public async Task<ActionResult<UploadOutcomeImportResponse>> Upload(
        [FromForm] UploadOutcomeImportForm form,
        CancellationToken cancellationToken)
    {
        await using var memory = new MemoryStream();
        await form.File.CopyToAsync(memory, cancellationToken);
        var result = await service.UploadAsync(
            form.CurriculumVersionId,
            form.SubjectExternalId,
            form.SubjectCode,
            form.SubjectName,
            form.File.FileName,
            form.File.ContentType,
            memory.ToArray(), Actor(), cancellationToken);
        return Created($"/api/management/outcome-imports/{result.Id}", result);
    }

    [HttpPost("outcome-imports/{id:long}/process")]
    public async Task<IActionResult> Process(long id, CancellationToken cancellationToken)
    {
        await service.ProcessAsync(id, cancellationToken);
        return Accepted($"/api/management/outcome-imports/{id}");
    }

    [HttpGet("outcome-imports/{id:long}")]
    public Task<OutcomeImportDetailDto> Detail(long id, CancellationToken cancellationToken) =>
        service.DetailAsync(id, cancellationToken);

    [HttpGet("outcome-imports/{id:long}/review")]
    public Task<OutcomeImportReviewDto> Review(long id, CancellationToken cancellationToken) =>
        service.ReviewAsync(id, cancellationToken);

    [HttpGet("outcome-imports/{id:long}/approved-document")]
    public Task<ApprovedOutcomeDocumentDto> ApprovedDocument(
        long id,
        CancellationToken cancellationToken) =>
        service.ApprovedDocumentAsync(id, cancellationToken);

    [HttpPatch("outcome-imports/{id:long}/review")]
    public Task<OutcomeImportReviewDto> Patch(
        long id,
        [FromBody] ReviewPatchRequest request,
        CancellationToken cancellationToken) =>
        service.PatchAsync(id, request, Actor(), cancellationToken);

    [HttpPost("outcome-imports/{id:long}/validate")]
    public Task<OutcomeImportReviewDto> Validate(
        long id,
        [FromBody] ValidateOutcomeImportRequest request,
        CancellationToken cancellationToken) =>
        service.ValidateAsync(id, request, Actor(), cancellationToken);

    [HttpPost("outcome-imports/{id:long}/approve")]
    public Task<OutcomeImportDetailDto> Approve(
        long id,
        [FromBody] ApproveOutcomeImportRequest request,
        CancellationToken cancellationToken) =>
        service.ApproveAsync(id, request, Actor(), cancellationToken);

    [HttpPost("outcome-imports/{id:long}/reject")]
    public Task<OutcomeImportDetailDto> Reject(
        long id,
        [FromBody] RejectOutcomeImportRequest request,
        CancellationToken cancellationToken) =>
        service.RejectAsync(id, request, Actor(), cancellationToken);

    [HttpPost("outcome-imports/{id:long}/archive")]
    public Task<OutcomeImportDetailDto> Archive(
        long id,
        [FromBody] ArchiveOutcomeImportRequest request,
        CancellationToken cancellationToken) =>
        service.ArchiveAsync(id, request, Actor(), cancellationToken);

    private ActorContext Actor() =>
        new(
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown",
            User.FindFirstValue(ClaimTypes.Name) ?? "System Administrator");
}

public sealed class UploadOutcomeImportForm
{
    public required IFormFile File { get; init; }
    public long CurriculumVersionId { get; init; }
    public Guid? SubjectExternalId { get; init; }
    public string? SubjectCode { get; init; }
    public string? SubjectName { get; init; }
}
