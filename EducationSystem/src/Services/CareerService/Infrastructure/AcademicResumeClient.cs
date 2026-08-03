using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareerService.Application;
using SharedKernel;

namespace CareerService.Infrastructure;

public sealed class AcademicResumeClient(HttpClient httpClient)
    : IAcademicResumeClient
{
    public async Task<AcademicResumeContextContract> GetContextAsync(
        Guid studentId,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"api/academic/resume/get-context-data/{studentId}");
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", bearerToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new DownstreamApiException(
                "STUDENT_CONTEXT_NOT_FOUND",
                "Academic resume context was not found.",
                StatusCodes.Status404NotFound);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            throw new DownstreamApiException(
                "ACADEMIC_AUTHORIZATION_FAILED",
                "AcademicService rejected the student access token.",
                StatusCodes.Status502BadGateway);
        if (!response.IsSuccessStatusCode)
            throw new DownstreamApiException(
                "ACADEMIC_SERVICE_UNAVAILABLE",
                "Academic context is currently unavailable.",
                StatusCodes.Status503ServiceUnavailable);

        return (await response.Content.ReadFromJsonAsync<
                    ApiResponse<AcademicResumeContextContract>>(cancellationToken))?.Data
               ?? throw new DownstreamApiException(
                   "ACADEMIC_INVALID_RESPONSE",
                   "AcademicService returned an invalid resume context.",
                   StatusCodes.Status502BadGateway);
    }
}
