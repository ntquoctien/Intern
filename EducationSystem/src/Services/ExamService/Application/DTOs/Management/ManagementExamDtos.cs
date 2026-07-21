namespace ExamService.Application.DTOs.Management;

public sealed record RawExamResultSummaryDto(int RecordCount, int ResultValueCount, float? MinimumRawResult, float? MaximumRawResult, double? AverageRawResult);
public sealed record ManagementRawExamResultDto(Guid ExamResultId, Guid StudentId, string ExamName, DateTime ExamDate, float? RawResult, float? RawCombinedResult, string? Description);
public sealed record QuestionSuiteSummaryDto(Guid QuestionSuiteId, Guid SubjectId, string Name, DateTime CreationTime, int QuestionCount);
public sealed record QuestionAnswerPreviewDto(Guid AnswerId, string AnswerText, string? ImageUrl, bool IsSourceMarkedAnswer);
public sealed record QuestionPreviewDto(Guid QuestionId, string QuestionText, int RawLevel, string? ImageUrl, IReadOnlyList<QuestionAnswerPreviewDto> Answers);
public sealed record QuestionSuiteDetailDto(Guid QuestionSuiteId, Guid SubjectId, string Name, DateTime CreationTime, IReadOnlyList<QuestionPreviewDto> Questions);
