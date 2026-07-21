using CommunicationService.Application.DTOs.StudentAccess;

namespace CommunicationService.Application.Interfaces;

public interface IStudentFormRequestService
{
    Task<IReadOnlyList<StudentFormRequestDto>> GetAllAsync(Guid studentId, CancellationToken cancellationToken);
    Task<StudentFormRequestDto?> GetByIdAsync(Guid studentId, Guid requestId, CancellationToken cancellationToken);
}
