using System.ComponentModel.DataAnnotations;

namespace AcademicService.Application.DTOs.Internships;

public sealed record SyncApprovedInternshipDto(
    Guid FormRequestId,
    Guid StudentId,
    [param: Required, StringLength(255)] string CompanyName,
    [param: Required, StringLength(100)] string Position,
    DateOnly StartDate,
    DateOnly? EndDate,
    [param: StringLength(8000)] string? TaskDescription);

public sealed record StudentInternshipDto(
    int InternshipId,
    Guid StudentId,
    string CompanyName,
    string Position,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? TaskDescription,
    Guid? FormRequestId);

public sealed record StudentInternshipVerificationProfileDto(
    Guid StudentId,
    string StudentCode,
    string StudentName);
