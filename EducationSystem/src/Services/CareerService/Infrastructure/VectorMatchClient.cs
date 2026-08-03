using System.Net;
using System.Net.Http.Json;
using CareerService.Application;
using Microsoft.Extensions.Options;

namespace CareerService.Infrastructure;

public sealed class VectorMatchClient(
    HttpClient httpClient,
    IOptions<VectorMatchClientOptions> options) : IVectorMatchClient
{
    public async Task<VectorMatchResponseContract> MatchAsync(
        VectorMatchRequestContract request,
        CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            "api/ai/vector-match/clos")
        {
            Content = JsonContent.Create(request)
        };
        if (!string.IsNullOrWhiteSpace(options.Value.InternalApiKey))
            message.Headers.TryAddWithoutValidation(
                "X-Internal-Api-Key",
                options.Value.InternalApiKey);

        using var response = await httpClient.SendAsync(message, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new DownstreamApiException(
                "VECTOR_MATCH_UNAVAILABLE",
                response.StatusCode == HttpStatusCode.UnprocessableEntity
                    ? "The vector match request was rejected."
                    : "Semantic matching is currently unavailable.",
                response.StatusCode == HttpStatusCode.UnprocessableEntity
                    ? StatusCodes.Status400BadRequest
                    : StatusCodes.Status503ServiceUnavailable);

        return await response.Content.ReadFromJsonAsync<VectorMatchResponseContract>(
                   cancellationToken)
               ?? throw new DownstreamApiException(
                   "VECTOR_MATCH_INVALID_RESPONSE",
                   "VectorMatchService returned an invalid response.",
                   StatusCodes.Status502BadGateway);
    }
}
