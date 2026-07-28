using AcademicService.Application.DTOs.Internships;

namespace AcademicService.Application.Interfaces;

public interface IStudentInternshipService
{
    Task<int?> SyncApprovedAsync(
        SyncApprovedInternshipDto request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<StudentInternshipDto>> GetByStudentAsync(
        Guid studentId,
        CancellationToken cancellationToken);

    Task<StudentInternshipVerificationProfileDto?> GetVerificationProfileAsync(
        Guid studentId,
        CancellationToken cancellationToken);
}
