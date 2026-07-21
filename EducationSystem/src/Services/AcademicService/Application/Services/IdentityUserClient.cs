using System.Net.Http.Json;
using AcademicService.Application.DTOs.StudentAccess;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using SharedKernel;

namespace AcademicService.Application.Services;

public sealed class IdentityUserClient(HttpClient httpClient) : IIdentityUserClient
{
    public async Task<SafeIdentityUserDto?> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"api/internal/users/{userId}/student-profile", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<SafeIdentityUserDto>>(cancellationToken);
        return payload?.Data;
    }

    public async Task<IReadOnlyDictionary<Guid, SafeIdentityUserDto>> GetManyAsync(
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var ids = userIds.Distinct().Take(100).ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<Guid, SafeIdentityUserDto>();
        }

        var query = ids.Aggregate("api/internal/users/summaries", (path, id) =>
            QueryHelpers.AddQueryString(path, "ids", id.ToString()));
        var payload = await httpClient.GetFromJsonAsync<ApiResponse<List<SafeIdentityUserDto>>>(query, cancellationToken);
        return (payload?.Data ?? []).ToDictionary(user => user.UserId);
    }
}
