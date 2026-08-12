using System.Net;
using System.Text;
using CareerService.Application;
using CareerService.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CareerService.Tests;

public sealed class VaultOutcomeProviderTests
{
    [Fact]
    public async Task Extraction_UsesStreamingChatCompletionAndParsesSseDeltas()
    {
        var handler = new CaptureHandler(HttpStatusCode.OK);
        var provider = Provider(handler);

        var result = await provider.ExtractAsync(
            new OutcomeExtractionProviderRequest(
                "Extract outcomes.",
                "v1",
                """{"blocks":[]}""",
                2),
            CancellationToken.None);

        Assert.Equal("""{"subjects":[],"warnings":[]}""", result.Json);
        Assert.Equal("https://vault.test/v1/chat/completions", handler.RequestUri);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("test-secret", handler.AuthorizationParameter);
        Assert.Contains("\"stream\":true", handler.Body);
        Assert.Contains("\"response_format\":{\"type\":\"json_object\"}", handler.Body);
    }

    [Fact]
    public async Task Gpt56Extraction_UsesResponsesApiAndParsesSseDeltas()
    {
        var handler = new CaptureHandler(HttpStatusCode.OK);
        var provider = Provider(handler, model: "gpt-5.6-sol");

        var result = await provider.ExtractAsync(
            new OutcomeExtractionProviderRequest(
                "Extract outcomes.", "v1", """{"blocks":[]}""", 2),
            CancellationToken.None);

        Assert.Equal("""{"subjects":[],"warnings":[]}""", result.Json);
        Assert.Equal("https://vault.test/v1/responses", handler.RequestUri);
        Assert.Contains("\"instructions\"", handler.Body);
        Assert.Contains("\"input\"", handler.Body);
        Assert.DoesNotContain("\"messages\"", handler.Body);
    }

    [Fact]
    public async Task Extraction_ReportsGatewayTimeoutWithoutMisclassifyingResponse()
    {
        var handler = new CaptureHandler(
            HttpStatusCode.GatewayTimeout,
            """{"error":{"message":"upstream timed out"}}""");

        var error = await Assert.ThrowsAsync<OutcomeImportException>(() =>
            Provider(handler).ExtractAsync(
                new OutcomeExtractionProviderRequest("task", "v1", "{}", 2),
                CancellationToken.None));

        Assert.Equal("VAULT_LLM_REQUEST_FAILED", error.ErrorCode);
        Assert.Equal(502, error.StatusCode);
        Assert.Contains("HTTP 504", error.Message);
        Assert.Contains("upstream timed out", error.Message);
    }

    [Fact]
    public async Task Extraction_RetriesGatewayTimeoutWhenConfigured()
    {
        var handler = new CaptureHandler(
            HttpStatusCode.GatewayTimeout,
            """{"error":{"message":"upstream timed out"}}""",
            HttpStatusCode.OK);

        var result = await Provider(handler, maxRetries: 1).ExtractAsync(
            new OutcomeExtractionProviderRequest("task", "v1", "{}", 2),
            CancellationToken.None);

        Assert.Equal("""{"subjects":[],"warnings":[]}""", result.Json);
        Assert.Equal(2, handler.RequestCount);
    }

    [Fact]
    public async Task Extraction_RetriesPrematureResponseWhenConfigured()
    {
        var handler = new ThrowOnceHandler();

        var result = await Provider(handler, maxRetries: 1).ExtractAsync(
            new OutcomeExtractionProviderRequest("task", "v1", "{}", 2),
            CancellationToken.None);

        Assert.Equal("""{"subjects":[],"warnings":[]}""", result.Json);
        Assert.Equal(2, handler.RequestCount);
    }

    private static VaultOutcomeProvider Provider(
        HttpMessageHandler handler,
        int maxRetries = 0,
        string model = "kimi-k3") =>
        new(
            new HttpClient(handler),
            Options.Create(new VaultLlmOptions
            {
                BaseUrl = "https://vault.test/v1",
                Model = model,
                ApiKey = "test-secret",
                MaxRetries = maxRetries,
                MaxOutputTokens = 4000
            }),
            NullLogger<VaultOutcomeProvider>.Instance);

    private sealed class CaptureHandler(
        HttpStatusCode status,
        string? errorBody = null,
        params HttpStatusCode[] subsequentStatuses) : HttpMessageHandler
    {
        private int requestCount;

        public int RequestCount => requestCount;
        public string Body { get; private set; } = string.Empty;
        public string RequestUri { get; private set; } = string.Empty;
        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationParameter { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var currentStatus = requestCount == 0
                ? status
                : subsequentStatuses.Length == 0
                    ? status
                    : subsequentStatuses[
                        Math.Min(requestCount - 1, subsequentStatuses.Length - 1)];
            requestCount++;
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            RequestUri = request.RequestUri!.AbsoluteUri;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;

            var response = new HttpResponseMessage(currentStatus);
            if (currentStatus != HttpStatusCode.OK)
            {
                response.Content = new StringContent(errorBody ?? string.Empty);
                return response;
            }

            const string content = "{\"subjects\":[],\"warnings\":[]}";
            var split = content.Length / 2;
            var sse = RequestUri.EndsWith("/responses", StringComparison.Ordinal)
                ? $"data: {{\"type\":\"response.output_text.delta\",\"delta\":{System.Text.Json.JsonSerializer.Serialize(content[..split])}}}\n\n" +
                  $"data: {{\"type\":\"response.output_text.delta\",\"delta\":{System.Text.Json.JsonSerializer.Serialize(content[split..])}}}\n\n" +
                  "data: [DONE]\n\n"
                : $"data: {{\"choices\":[{{\"delta\":{{\"content\":{System.Text.Json.JsonSerializer.Serialize(content[..split])}}}}}]}}\n\n" +
                  $"data: {{\"choices\":[{{\"delta\":{{\"content\":{System.Text.Json.JsonSerializer.Serialize(content[split..])}}}}}]}}\n\n" +
                  "data: [DONE]\n\n";
            response.Content = new StringContent(sse, Encoding.UTF8, "text/event-stream");
            return response;
        }
    }

    private sealed class ThrowOnceHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            if (RequestCount == 1)
                throw new HttpIOException(
                    HttpRequestError.ResponseEnded,
                    "The response ended prematurely.");

            const string content = "{\"subjects\":[],\"warnings\":[]}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $"data: {{\"choices\":[{{\"delta\":{{\"content\":{System.Text.Json.JsonSerializer.Serialize(content)}}}}}]}}\n\n" +
                    "data: [DONE]\n\n",
                    Encoding.UTF8,
                    "text/event-stream")
            };
            return Task.FromResult(response);
        }
    }
}
