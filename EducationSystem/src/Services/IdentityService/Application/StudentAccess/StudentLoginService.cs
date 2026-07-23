using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace IdentityService.Application.StudentAccess;

public interface IStudentLoginService
{
    Task<StudentLoginResult> LoginAsync(string studentCode, string credential, CancellationToken cancellationToken = default);

    Task<StudentSessionDto?> GetSessionAsync(CurrentStudent currentStudent, DateTimeOffset expiresAt, CancellationToken cancellationToken = default);
}

public sealed class StudentLoginService(
    IdentityDbContext dbContext,
    IAcademicStudentIdentityClient academicClient,
    IStudentTokenIssuer tokenIssuer,
    IOptions<StudentLoginOptions> loginOptions) : IStudentLoginService
{
    public async Task<StudentLoginResult> LoginAsync(
        string studentCode,
        string credential,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = studentCode.Trim();
        if (normalizedCode.Length is < 1 or > 100 ||
            !string.Equals(credential, loginOptions.Value.DefaultCredential, StringComparison.Ordinal))
        {
            return Failure(StudentErrorCodes.NotFound, "Student code was not found.");
        }

        AcademicStudentResolution resolution;
        try
        {
            resolution = loginOptions.Value.CodeSource == StudentCodeSource.Nickname
                ? await academicClient.ResolveByNicknameAsync(normalizedCode, cancellationToken)
                : await ResolveViaUserAsync(normalizedCode, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return Failure(StudentErrorCodes.IdentityServiceUnavailable, "Student identity lookup is temporarily unavailable.");
        }

        if (resolution.MatchCount == 0 || resolution.Candidate is null && resolution.MatchCount < 2)
        {
            return Failure(StudentErrorCodes.NotFound, "Student code was not found.");
        }

        if (resolution.MatchCount > 1)
        {
            return Failure(StudentErrorCodes.CodeAmbiguous, "Student code maps to multiple records.");
        }

        var candidate = resolution.Candidate!;
        var users = await dbContext.Users.AsNoTracking()
            .Where(user => user.Id == candidate.UserId)
            .Take(2)
            .Select(user => new SafeUserProfileDto(
                user.Id,
                user.FullName,
                user.UserName,
                user.ProfilePicUrl,
                user.IsActived && !user.IsDeleted))
            .ToListAsync(cancellationToken);

        if (users.Count != 1 || !users[0].IsActive)
        {
            return Failure(StudentErrorCodes.AccountUnavailable, "The linked student account is unavailable.");
        }

        var userProfile = users[0];
        var issued = tokenIssuer.Issue(candidate.StudentId, userProfile.UserId, normalizedCode);
        var session = new StudentSessionDto(
            candidate.StudentId,
            userProfile.UserId,
            normalizedCode,
            userProfile.FullName,
            userProfile.UserName,
            userProfile.ProfilePicUrl,
            issued.ExpiresAt);
        return new StudentLoginResult(new StudentLoginResponse(issued.AccessToken, session), null, null);
    }

    public Task<StudentSessionDto?> GetSessionAsync(
        CurrentStudent currentStudent,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default) =>
        dbContext.Users.AsNoTracking()
            .Where(user => user.Id == currentStudent.UserId && user.IsActived && !user.IsDeleted)
            .Select(user => new StudentSessionDto(
                currentStudent.StudentId,
                user.Id,
                currentStudent.StudentCode,
                user.FullName,
                user.UserName,
                user.ProfilePicUrl,
                expiresAt))
            .SingleOrDefaultAsync(cancellationToken);

    private async Task<AcademicStudentResolution> ResolveViaUserAsync(
        string studentCode,
        CancellationToken cancellationToken)
    {
        var source = loginOptions.Value.CodeSource;
        var users = await dbContext.Users.AsNoTracking()
            .Where(user => source == StudentCodeSource.UserInternalId
                ? user.UserInternalId == studentCode
                : user.UserName == studentCode)
            .OrderBy(user => user.Id)
            .Take(2)
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

        if (users.Count != 1)
        {
            return new AcademicStudentResolution(users.Count, null);
        }

        return await academicClient.ResolveByUserIdAsync(users[0], cancellationToken);
    }

    private static StudentLoginResult Failure(string code, string message) => new(null, code, message);
}
