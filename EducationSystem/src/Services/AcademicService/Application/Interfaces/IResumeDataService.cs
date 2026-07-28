using AcademicService.Application.DTOs.Resume;

namespace AcademicService.Application.Interfaces;

public interface IResumeDataService
{
    Task<ResumeContextDto?> GetContextDataAsync(
        Guid studentId,
        CancellationToken cancellationToken);
}
