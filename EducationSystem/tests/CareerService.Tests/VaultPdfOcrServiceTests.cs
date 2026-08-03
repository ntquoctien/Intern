using System.Net;
using System.Text;
using CareerService.Application;
using CareerService.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CareerService.Tests;

public sealed class VaultPdfOcrServiceTests
{
    [Fact]
    public async Task Recognition_UsesDedicatedOcrSettingsAndVaultConnection()
    {
        var handler = new CaptureHandler(HttpStatusCode.OK);
        var service = Service(handler, maxRetries: 0);

        var blocks = await service.RecognizeAsync(
            "%PDF-scanned"u8.ToArray(),
            CancellationToken.None);

        Assert.Single(blocks);
        Assert.Equal("https://vault.test/v1/responses", handler.RequestUri);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("vault-secret", handler.AuthorizationParameter);
        Assert.Contains("\"model\":\"ocr-model\"", handler.Bodies.Single());
        Assert.Contains("\"max_output_tokens\":8192", handler.Bodies.Single());
    }

    [Fact]
    public async Task Recognition_RetriesGatewayTimeoutUsingOcrRetrySettings()
    {
        var handler = new CaptureHandler(
            HttpStatusCode.GatewayTimeout,
            HttpStatusCode.OK);

        var blocks = await Service(handler, maxRetries: 1).RecognizeAsync(
            "%PDF-scanned"u8.ToArray(),
            CancellationToken.None);

        Assert.Single(blocks);
        Assert.Equal(2, handler.Bodies.Count);
    }

    private static VaultPdfOcrService Service(
        CaptureHandler handler,
        int maxRetries) =>
        new(
            new HttpClient(handler),
            Options.Create(new VaultLlmOptions
            {
                BaseUrl = "https://vault.test/v1",
                Model = "llm-model-must-not-be-used-for-ocr",
                ApiKey = "vault-secret",
                MaxRetries = 0,
                MaxOutputTokens = 4000
            }),
            Options.Create(new PdfOcrOptions
            {
                Provider = "Vault",
                Model = "ocr-model",
                MaxRetries = maxRetries,
                MaxOutputTokens = 8192
            }),
            NullLogger<VaultPdfOcrService>.Instance);

    private sealed class CaptureHandler(params HttpStatusCode[] statuses)
        : HttpMessageHandler
    {
        private int requestCount;

        public List<string> Bodies { get; } = [];
        public string RequestUri { get; private set; } = string.Empty;
        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationParameter { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Bodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            RequestUri = request.RequestUri!.AbsoluteUri;
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;

            var status = statuses[Math.Min(requestCount++, statuses.Length - 1)];
            if (status != HttpStatusCode.OK)
                return new HttpResponseMessage(status)
                {
                    Content = new StringContent(
                        """{"error":{"message":"upstream timed out"}}""")
                };

            const string ocrJson =
                """{"pages":[{"pageNumber":1,"text":"CLO1 Vận dụng kiến thức"}]}""";
            var escaped = System.Text.Json.JsonSerializer.Serialize(ocrJson);
            var sse =
                $"data: {{\"type\":\"response.output_text.delta\",\"delta\":{escaped}}}\n\n" +
                "data: {\"type\":\"response.completed\"}\n\n";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(sse, Encoding.UTF8, "text/event-stream")
            };
        }
    }
}