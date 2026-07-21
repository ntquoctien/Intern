using AcademicService.Application.DTOs.StudentAccess;

namespace AcademicService.Application.Interfaces;

public interface IIdentityUserClient
{
    Task<SafeIdentityUserDto?> GetAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, SafeIdentityUserDto>> GetManyAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken);
}
