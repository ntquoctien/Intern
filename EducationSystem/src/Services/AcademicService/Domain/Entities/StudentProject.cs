namespace EducationSystem.Services.Academic.Domain.Entities;

/// <summary>
/// A student-owned e-Portfolio project that can be selected as source data for a CV.
/// </summary>
public sealed class StudentProject
{
    public int ProjectId { get; set; }

    // The current academic.Students primary key is uniqueidentifier.
    public Guid StudentId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string TechStack { get; set; } = null!;

    public string? ProjectDescription { get; set; }

    public string? SourceCodeUrl { get; set; }

    public int TeamSize { get; set; } = 1;

    public string? MyRole { get; set; }

    public string? MyContributions { get; set; }

    // The current academic.Subjects primary key is uniqueidentifier.
    public Guid? MappedCourseId { get; set; }
}
