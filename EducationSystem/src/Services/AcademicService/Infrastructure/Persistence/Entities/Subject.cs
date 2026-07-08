using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class Subject
{
    public Guid Id { get; set; }

    public Guid? FacultyId { get; set; }

    public string SubjectCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int CreditPoint { get; set; }

    public int? TotalHours { get; set; }

    public string Note { get; set; } = null!;

    public bool IsActived { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Faculty? Faculty { get; set; }

    public virtual ICollection<SemesterSubject> SemesterSubjects { get; set; } = new List<SemesterSubject>();

    public virtual ICollection<SubjectDocument> SubjectDocuments { get; set; } = new List<SubjectDocument>();

    public virtual ICollection<SubjectSpecialNote> SubjectSpecialNotes { get; set; } = new List<SubjectSpecialNote>();

    public virtual ICollection<SubjectTeaching> SubjectTeachings { get; set; } = new List<SubjectTeaching>();
}
