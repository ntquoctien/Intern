using ExamService.Application.DTOs.Management;
using ExamService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace ExamService.Controllers;

[ApiController]
[Route("api/management")]
public sealed class ManagementExamReadController(IManagementExamReadService service) : ControllerBase
{
    [HttpGet("exam-results/summary")] public async Task<ApiResponse<RawExamResultSummaryDto>> Summary(CancellationToken ct) => ApiResponse<RawExamResultSummaryDto>.Ok(await service.GetResultSummaryAsync(ct));
    [HttpGet("exam-results")] public async Task<ApiResponse<IReadOnlyList<ManagementRawExamResultDto>>> Results(CancellationToken ct) => ApiResponse<IReadOnlyList<ManagementRawExamResultDto>>.Ok(await service.GetResultsAsync(ct));
    [HttpGet("question-suites")] public async Task<ApiResponse<IReadOnlyList<QuestionSuiteSummaryDto>>> Suites(CancellationToken ct) => ApiResponse<IReadOnlyList<QuestionSuiteSummaryDto>>.Ok(await service.GetSuitesAsync(ct));
    [HttpGet("question-suites/{id:guid}")] public async Task<ActionResult<ApiResponse<QuestionSuiteDetailDto>>> Suite(Guid id, CancellationToken ct) { var item = await service.GetSuiteAsync(id, ct); return item is null ? NotFound(ApiResponse<QuestionSuiteDetailDto>.Fail("Question suite not found.")) : Ok(ApiResponse<QuestionSuiteDetailDto>.Ok(item)); }
    [HttpGet("exams-page")] public async Task<ApiResponse<ManagementExamPageDto>> Exams([FromQuery] ManagementExamQueryDto query, CancellationToken ct) => ApiResponse<ManagementExamPageDto>.Ok(await service.GetExamPageAsync(query, ct));
    [HttpGet("exams-page/{id:guid}")] public async Task<ActionResult<ApiResponse<ManagementExamItemDto>>> Exam(Guid id, CancellationToken ct) { var item = await service.GetExamAsync(id, ct); return item is null ? NotFound(ApiResponse<ManagementExamItemDto>.Fail("Exam not found.")) : Ok(ApiResponse<ManagementExamItemDto>.Ok(item)); }
    [HttpGet("questions-page")] public async Task<ApiResponse<QuestionBankPageDto>> Questions([FromQuery] QuestionBankQueryDto query, CancellationToken ct) => ApiResponse<QuestionBankPageDto>.Ok(await service.GetQuestionPageAsync(query, ct));
    [HttpGet("questions-page/{id:guid}")] public async Task<ActionResult<ApiResponse<QuestionBankDetailDto>>> Question(Guid id, CancellationToken ct) { var item = await service.GetQuestionAsync(id, ct); return item is null ? NotFound(ApiResponse<QuestionBankDetailDto>.Fail("Question not found.")) : Ok(ApiResponse<QuestionBankDetailDto>.Ok(item)); }
}
