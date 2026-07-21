using AcademicService.Application.DTOs.StudentAccess;

namespace AcademicService.Application.Interfaces;

public interface IStudentIdentityResolver
{
    Task<StudentIdentityResolutionDto> ResolveByNicknameAsync(string code, CancellationToken cancellationToken = default);

    Task<StudentIdentityResolutionDto> ResolveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
