using CommunicationService.Application.DTOs.StudentAccess;

namespace CommunicationService.Application.Interfaces;

public interface IStudentAnnouncementService
{
    Task<IReadOnlyList<StudentAnnouncementDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
}
