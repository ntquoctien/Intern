namespace ExamService.Application.DTOs.Management;

public sealed record RawExamResultSummaryDto(int RecordCount, int ResultValueCount, float? MinimumRawResult, float? MaximumRawResult, double? AverageRawResult);
public sealed record ManagementRawExamResultDto(Guid ExamResultId, Guid StudentId, string ExamName, DateTime ExamDate, float? RawResult, float? RawCombinedResult, string? Description);
public sealed record QuestionSuiteSummaryDto(Guid QuestionSuiteId, Guid SubjectId, string Name, DateTime CreationTime,
    int QuestionCount, Guid? UpdatedById, int Level0Count, int Level1Count, int Level2Count, int Level3Count);
public sealed record QuestionAnswerPreviewDto(Guid AnswerId, string AnswerText, string? ImageUrl, bool IsSourceMarkedAnswer);
public sealed record QuestionPreviewDto(Guid QuestionId, string QuestionText, int RawLevel, string? ImageUrl, IReadOnlyList<QuestionAnswerPreviewDto> Answers);
public sealed record QuestionSuiteDetailDto(Guid QuestionSuiteId, Guid SubjectId, string Name, DateTime CreationTime, IReadOnlyList<QuestionPreviewDto> Questions);
public sealed record ManagementExamQueryDto(int PageNumber = 1, int PageSize = 10, string? Search = null,
    Guid? SubjectTeachingId = null, int? Type = null, DateTime? FromDate = null, DateTime? ToDate = null);
public sealed record ManagementExamItemDto(Guid ExamId, Guid SubjectTeachingId, Guid? QuestionSuiteId,
    string? QuestionSuiteName, string Name, DateTime StartDate, DateTime EndDate, Guid? RoomId,
    Guid? TeacherId, string Notes, int Type, int QuestionCount, int EasyCount, int NormalCount,
    int HardCount, int PracticeCount, int? Method, bool AllowNotifyStudent, int AttemptCount, int ResultCount);
public sealed record ManagementExamPageDto(IReadOnlyList<ManagementExamItemDto> Items, int PageNumber,
    int PageSize, int TotalItems, int TotalAttempts, int TotalResults, int ResultsWithScore);
public sealed record QuestionBankQueryDto(int PageNumber = 1, int PageSize = 10, Guid? SuiteId = null,
    int? Level = null, string? Search = null);
public sealed record QuestionBankItemDto(Guid QuestionId, Guid SuiteId, string SuiteName, Guid SubjectId,
    string QuestionText, int RawLevel, string? ImageUrl, int AnswerCount);
public sealed record QuestionBankAnswerDto(Guid AnswerId, string AnswerText, string? ImageUrl);
public sealed record QuestionBankDetailDto(Guid QuestionId, Guid SuiteId, string SuiteName, Guid SubjectId,
    string QuestionText, int RawLevel, string? ImageUrl, IReadOnlyList<QuestionBankAnswerDto> Answers);
public sealed record QuestionBankPageDto(IReadOnlyList<QuestionBankItemDto> Items, int PageNumber,
    int PageSize, int TotalItems);
