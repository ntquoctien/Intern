using System.ComponentModel.DataAnnotations;

namespace CommunicationService.Application.DTOs.InternshipVerification;

public sealed record SubmitInternshipRequestDto(
    [param: Required, StringLength(255)] string CompanyName,
    [param: Required, StringLength(100)] string Position,
    [param: Required, EmailAddress, StringLength(320)] string MentorEmail,
    DateOnly StartDate,
    DateOnly? EndDate,
    [param: StringLength(8000)] string? TaskDescription);

public sealed record VerifyInternshipByEmployerDto(
    Guid Token,
    bool IsInformationCorrect,
    [param: Required, Range(typeof(decimal), "0", "10")] decimal? Score,
    [param: StringLength(4000)] string? EvaluationNotes);

public sealed record InternshipRequestCreatedDto(
    Guid RequestId,
    string VerificationStatus,
    DateTime CreatedAt);

public sealed record InternshipVerificationResultDto(
    Guid RequestId,
    int EmployerVerifiedStatus,
    DateTime VerifiedAt);

public sealed record InternshipVerificationContextDto(
    string CompanyName,
    string StudentName,
    string StudentCode,
    string Position,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? TaskDescription);

public sealed record InternshipApprovalResultDto(
    Guid RequestId,
    int InternshipId,
    DateTime ApprovedAt);

public sealed record InternshipVerificationDocument(
    InternshipDeclaration Declaration,
    EmployerAssessment? EmployerAssessment,
    InternshipStudentSnapshot? Student = null);

public sealed record InternshipDeclaration(
    string CompanyName,
    string Position,
    string MentorEmail,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? TaskDescription);

public sealed record EmployerAssessment(
    bool IsInformationCorrect,
    decimal Score,
    string? EvaluationNotes,
    DateTime VerifiedAt);

public sealed record InternshipStudentSnapshot(
    string StudentName,
    string StudentCode);

public sealed record CreateApprovedInternshipDto(
    Guid FormRequestId,
    Guid StudentId,
    string CompanyName,
    string Position,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? TaskDescription);

public sealed record AcademicStudentVerificationProfileDto(
    Guid StudentId,
    string StudentCode,
    string StudentName);

public static class FormRequestWorkflowStatuses
{
    public const int Pending = 0;
    public const int Approved = 2;
}
