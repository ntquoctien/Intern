namespace EducationSystem.Services.Academic.Domain.Entities;

/// <summary>
/// An official internship history item created after a form request is approved.
/// </summary>
public sealed class StudentInternship
{
    public int InternshipId { get; set; }

    // The current academic.Students primary key is uniqueidentifier.
    public Guid StudentId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string Position { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? TaskDescription { get; set; }

    /// <summary>
    /// Correlation identifier of communication.FormRequests.Id.
    /// It intentionally has no EF navigation because service schemas are isolated.
    /// </summary>
    public Guid? FormRequestId { get; set; }
}
