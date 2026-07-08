namespace AcademicService.Application.DTOs.Students;

public sealed class StudentListItemDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid AcademicYearId { get; init; }
    public Guid MajorId { get; init; }
    public Guid? RelativeUserId { get; init; }
    public int? StudyStatus { get; init; }
    public int? Gender { get; init; }
    public string? Nickname { get; init; }
    public string? PlaceOfBirth { get; init; }
    public string? Hometown { get; init; }
    public string? PermanentAddress { get; init; }
    public string? ContactAddress { get; init; }
    public string? Ethnicity { get; init; }
    public string? Religion { get; init; }
    public string? EducationLevel { get; init; }
    public string? FatherName { get; init; }
    public string? FatherOccupation { get; init; }
    public string? MotherName { get; init; }
    public string? MotherOccupation { get; init; }
    public string? SpouseName { get; init; }
    public string? SpouseOccupation { get; init; }
    public string? PolicySubject { get; init; }
    public string? PreviousOccupation { get; init; }
    public string? PostGraduationWorkplace { get; init; }
    public DateTime? CommunistPartyJoinDate { get; init; }
    public DateTime? OfficialPartyJoinDate { get; init; }
    public DateTime? YouthUnionJoinDate { get; init; }
    public bool IsGraduated { get; init; }
    public bool? HasIssue { get; init; }
    public string IssueDescription { get; init; } = string.Empty;
    public int? LibraryId { get; init; }
    public bool IsDeleted { get; init; }
}
