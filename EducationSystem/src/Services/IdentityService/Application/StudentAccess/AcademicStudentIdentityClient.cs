using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using SharedKernel;

namespace IdentityService.Application.StudentAccess;

public interface IAcademicStudentIdentityClient
{
    Task<AcademicStudentResolution> ResolveByNicknameAsync(string code, CancellationToken cancellationToken);

    Task<AcademicStudentResolution> ResolveByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}

public sealed class AcademicStudentIdentityClient(HttpClient httpClient) : IAcademicStudentIdentityClient
{
    public async Task<AcademicStudentResolution> ResolveByNicknameAsync(
        string code,
        CancellationToken cancellationToken)
    {
        var path = QueryHelpers.AddQueryString("api/internal/student-identities/by-nickname", "code", code);
        return await GetResolutionAsync(path, cancellationToken);
    }

    public Task<AcademicStudentResolution> ResolveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        GetResolutionAsync($"api/internal/student-identities/by-user/{userId}", cancellationToken);

    private async Task<AcademicStudentResolution> GetResolutionAsync(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AcademicStudentResolution>>(
            cancellationToken: cancellationToken);
        return payload?.Data ?? throw new HttpRequestException("Academic identity lookup returned an invalid response.");
    }
}
