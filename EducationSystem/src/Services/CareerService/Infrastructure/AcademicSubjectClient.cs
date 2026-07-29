using System.Net.Http.Json;
using System.Text.Json;
using CareerService.Application;
using Microsoft.Extensions.Options;

namespace CareerService.Infrastructure;

public sealed class AcademicSubjectClient(
    HttpClient httpClient,
    IOptions<AcademicClientOptions> options,
    ILogger<AcademicSubjectClient> logger)
{
    public async Task<IReadOnlyDictionary<string, AcademicSubjectMatch>> GetSubjectsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var url = $"{options.Value.BaseUrl.TrimEnd('/')}/api/management/subjects";
            using var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var json = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
            var data = json.RootElement.GetProperty("data");
            return data.EnumerateArray()
                .Select(item => new AcademicSubjectMatch(
                    item.GetProperty("subjectId").GetGuid(),
                    item.GetProperty("subjectCode").GetString() ?? string.Empty,
                    item.GetProperty("name").GetString() ?? string.Empty,
                    item.TryGetProperty("creditPoint", out var credits) ? credits.GetInt32() : null))
                .Where(item => item.Code.Length > 0)
                .GroupBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or JsonException or InvalidOperationException)
        {
            logger.LogWarning(exception, "Academic subject lookup is unavailable.");
            return new Dictionary<string, AcademicSubjectMatch>(StringComparer.OrdinalIgnoreCase);
        }
    }
}

public sealed record AcademicSubjectMatch(Guid Id, string Code, string Name, int? Credits);
