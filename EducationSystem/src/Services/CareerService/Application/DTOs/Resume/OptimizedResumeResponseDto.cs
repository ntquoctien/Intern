namespace CareerService.Application.DTOs.Resume;

public sealed class OptimizedResumeResponseDto
{
    public StudentHeaderDto Header { get; set; } = new();
    public EducationBlockDto Education { get; set; } = new();
    public string ProfessionalSummary { get; set; } = string.Empty;
    public CategorizedSkillsDto Skills { get; set; } = new();
    public List<OptimizedProjectDto> Projects { get; set; } = [];
    public List<OptimizedInternshipDto> Internships { get; set; } = [];
    public List<string> Certifications { get; set; } = [];
    public List<string> AwardsAndActivities { get; set; } = [];
    public ResumeQualityMetricsDto QualityMetrics { get; set; } = new();
}

public sealed class StudentHeaderDto
{
    public string FullName { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public double? Gpa { get; set; }
    public string TargetRole { get; set; } = string.Empty;
}

public sealed class EducationBlockDto
{
    public string InstitutionName { get; set; } = string.Empty;
    public string MajorName { get; set; } = string.Empty;
    public string DegreeName { get; set; } = "Cử nhân / Kỹ sư";
    public double? Gpa { get; set; }
    public string DurationText { get; set; } = string.Empty;
}

public sealed class CategorizedSkillsDto
{
    public List<SkillItemDto> KnowledgeDomain { get; set; } = [];
    public List<SkillItemDto> FunctionalSkills { get; set; } = [];
    public List<SkillItemDto> InterpersonalSkills { get; set; } = [];
}

public sealed class SkillItemDto
{
    public string SkillName { get; set; } = string.Empty;
    public string Proficiency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class OptimizedProjectDto
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string TechStack { get; set; } = string.Empty;
    public string MyRole { get; set; } = string.Empty;
    public List<string> ActionBulletPoints { get; set; } = [];
}

public sealed class OptimizedInternshipDto
{
    public int InternshipId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string DurationText { get; set; } = string.Empty;
    public List<string> ActionBulletPoints { get; set; } = [];
}

public sealed class ResumeQualityMetricsDto
{
    public double JobAlignmentScore { get; set; }
    public double ContentPreservationScore { get; set; }
    public bool HasHallucinationWarning { get; set; }
}
