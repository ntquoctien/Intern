using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class Student
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid AcademicYearId { get; set; }

    public Guid MajorId { get; set; }

    public Guid? RelativeUserId { get; set; }

    public int? StudyStatus { get; set; }

    public int? Gender { get; set; }

    public string? Nickname { get; set; }

    public string? PlaceOfBirth { get; set; }

    public string? Hometown { get; set; }

    public string? PermanentAddress { get; set; }

    public string? ContactAddress { get; set; }

    public string? Ethnicity { get; set; }

    public string? Religion { get; set; }

    public string? EducationLevel { get; set; }

    public string? FatherName { get; set; }

    public string? FatherOccupation { get; set; }

    public string? MotherName { get; set; }

    public string? MotherOccupation { get; set; }

    public string? SpouseName { get; set; }

    public string? SpouseOccupation { get; set; }

    public string? PolicySubject { get; set; }

    public string? PreviousOccupation { get; set; }

    public string? PostGraduationWorkplace { get; set; }

    public DateTime? CommunistPartyJoinDate { get; set; }

    public DateTime? OfficialPartyJoinDate { get; set; }

    public DateTime? YouthUnionJoinDate { get; set; }

    public bool IsGraduated { get; set; }

    public bool? HasIssue { get; set; }

    public string IssueDescription { get; set; } = null!;

    public int? LibraryId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual AcademicYear AcademicYear { get; set; } = null!;

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Major Major { get; set; } = null!;

    public virtual ICollection<SemesterTuition> SemesterTuitions { get; set; } = new List<SemesterTuition>();

    public virtual ICollection<StudentEvaluation> StudentEvaluations { get; set; } = new List<StudentEvaluation>();

    public virtual ICollection<SubjectSpecialNote> SubjectSpecialNotes { get; set; } = new List<SubjectSpecialNote>();

    public virtual ICollection<SubjectStudent> SubjectStudents { get; set; } = new List<SubjectStudent>();
}
