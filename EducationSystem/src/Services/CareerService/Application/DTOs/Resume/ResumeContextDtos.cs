using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CareerService.Application.DTOs.Resume;

public sealed class PrepareResumePayloadRequestDto
{
    public Guid StudentId { get; set; }

    [Required, StringLength(200, MinimumLength = 2)]
    public string TargetRole { get; set; } = string.Empty;

    [Required, StringLength(10_000, MinimumLength = 10)]
    public string JobDescription { get; set; } = string.Empty;

    [StringLength(200)]
    public string? CareerFocusTag { get; set; }

    public List<Guid> SelectedSubjectIds { get; set; } = [];

    public List<int> SelectedInternshipIds { get; set; } = [];

    public List<UiProjectOverrideDto>? UiProjects { get; set; }

    public List<string> Certifications { get; set; } = [];

    public List<string> AwardsAndActivities { get; set; } = [];

    [StringLength(3_000)]
    public string? CurrentSummaryDraft { get; set; }

    [Range(1, 100)]
    public int TopK { get; set; } = 8;

    [Range(0.50, 1.0)]
    public float SimilarityThreshold { get; set; } = 0.65f;
}

public sealed class UiProjectOverrideDto
{
    public int? ProjectId { get; set; }

    [Required, StringLength(255)]
    public string ProjectName { get; set; } = string.Empty;

    [StringLength(500)]
    public string TechStack { get; set; } = string.Empty;

    [StringLength(200)]
    public string MyRole { get; set; } = string.Empty;

    [StringLength(4_000)]
    public string MyContributions { get; set; } = string.Empty;

    [Range(1, 100)]
    public int TeamSize { get; set; } = 1;
}

public sealed class ResumePromptPayloadDto
{
    public StudentInfoPromptDto StudentInfo { get; set; } = new();
    public string TargetRole { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public string? CareerFocusTag { get; set; }
    public string? CurrentSummaryDraft { get; set; }
    public List<HydratedMatchedSubjectDto> MatchedSubjects { get; set; } = [];
    public List<HydratedProjectDto> Projects { get; set; } = [];
    public List<HydratedInternshipDto> Internships { get; set; } = [];
    public List<string> Certifications { get; set; } = [];
    public List<string> AwardsAndActivities { get; set; } = [];
    public bool IsFallbackMode { get; set; }
}

public sealed class StudentInfoPromptDto
{
    // Needed only for server-side ownership/header restoration. These fields
    // are intentionally excluded from both BFF JSON and the LLM prompt.
    [JsonIgnore]
    public Guid StudentId { get; set; }

    [JsonIgnore]
    public string StudentCode { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public string? FacultyName { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public double? Gpa { get; set; }
}

public sealed class HydratedMatchedSubjectDto
{
    public Guid SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int CreditPoint { get; set; }
    public double Score { get; set; }
    public List<HydratedOutcomeDto> CourseOutcomes { get; set; } = [];
}

public sealed class HydratedOutcomeDto
{
    public string OutcomeCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public float SimilarityScore { get; set; }
    public string ProgressionLevel { get; set; } = string.Empty;
}

public sealed class HydratedProjectDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string TechStack { get; set; } = string.Empty;
    public string ProjectDescription { get; set; } = string.Empty;
    public string MyRole { get; set; } = string.Empty;
    public string MyContributions { get; set; } = string.Empty;
    public int TeamSize { get; set; } = 1;
    public string? MappedCourseCode { get; set; }
}

public sealed class HydratedInternshipDto
{
    public int InternshipId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string TaskDescription { get; set; } = string.Empty;
}
