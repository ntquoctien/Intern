using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CareerService.Application;
using Microsoft.Extensions.Options;

namespace CareerService.Infrastructure;

public sealed class VaultOutcomeProvider(
    HttpClient httpClient,
    IOptions<VaultLlmOptions> options,
    ILogger<VaultOutcomeProvider> logger)
    : IOutcomeExtractionProvider, IAiOutcomeMappingProvider
{
    public Task<OutcomeExtractionProviderResult> MapAsync(
        OutcomeExtractionProviderRequest request,
        CancellationToken cancellationToken) =>
        ExtractAsync(request, cancellationToken);

    public async Task<OutcomeExtractionProviderResult> ExtractAsync(
        OutcomeExtractionProviderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.RequestNumber is < 1 or > 4)
            throw new OutcomeImportException(
                "LLM_TASK_NOT_ALLOWED",
                "Only configured outcome extraction and mapping tasks may invoke Vault LLM.",
                400);

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey) ||
            string.IsNullOrWhiteSpace(settings.Model))
            throw new OutcomeImportException(
                "VAULT_LLM_NOT_CONFIGURED",
                "Vault LLM requires VaultLLM:Model and VaultLLM:ApiKey.",
                503);

        var schema = GeminiOutcomeExtractionProvider
            .BuildResponseSchema(request.RequestNumber)
            .ToJsonString();
        var prompt =
            $"""
             {GeminiOutcomeExtractionProvider.BuildPrompt(request)}

             Return one JSON object matching this schema. Do not wrap it in markdown:
             {schema}
             """;
        var payload = new
        {
            model = settings.Model,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = GeminiOutcomeExtractionProvider.SystemInstruction
                },
                new { role = "user", content = prompt }
            },
            temperature = 0.1,
            max_completion_tokens = settings.MaxOutputTokens,
            stream = true,
            response_format = new { type = "json_object" }
        };

        Exception? last = null;
        for (var attempt = 0; attempt <= settings.MaxRetries; attempt++)
        {
            try
            {
                using var message = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{settings.BaseUrl.TrimEnd('/')}/chat/completions")
                {
                    Content = JsonContent.Create(payload)
                };
                message.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", settings.ApiKey);
                using var response = await httpClient.SendAsync(
                    message,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                var responseText = await ReadResponseBodyAsync(
                    response,
                    cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    if ((response.StatusCode == HttpStatusCode.TooManyRequests ||
                         (int)response.StatusCode >= 500) &&
                        attempt < settings.MaxRetries)
                    {
                        await Task.Delay(
                            TimeSpan.FromSeconds(Math.Pow(2, attempt + 1)),
                            cancellationToken);
                        continue;
                    }
                    throw new OutcomeImportException(
                        response.StatusCode == HttpStatusCode.TooManyRequests
                            ? "VAULT_LLM_RATE_LIMIT"
                            : "VAULT_LLM_REQUEST_FAILED",
                        $"Vault LLM request failed with HTTP {(int)response.StatusCode}." +
                        ProviderDetail(responseText),
                        response.StatusCode == HttpStatusCode.TooManyRequests ? 429 : 502);
                }

                var content = VaultChatResponseParser.ExtractContent(responseText);
                var normalized = NormalizeOptionalFields(
                    content, request.RequestNumber);
                using var structured = JsonDocument.Parse(normalized);
                GeminiOutcomeExtractionProvider.EnsureExpectedShape(
                    structured.RootElement, request.RequestNumber);
                return new OutcomeExtractionProviderResult(
                    structured.RootElement.GetRawText(),
                    response.Headers.TryGetValues("x-request-id", out var values)
                        ? values.FirstOrDefault()
                        : null);
            }
            catch (Exception exception) when (
                exception is HttpRequestException or TaskCanceledException or JsonException)
            {
                last = exception;
                if (attempt < settings.MaxRetries)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(Math.Pow(2, attempt + 1)),
                        cancellationToken);
                    continue;
                }
            }
        }

        logger.LogWarning(last, "Vault LLM returned an invalid structured response.");
        throw new OutcomeImportException(
            "VAULT_LLM_INVALID_RESPONSE",
            "Vault LLM did not return valid structured JSON.",
            502,
            last);
    }

    private static async Task<string> ReadResponseBodyAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
            return await response.Content.ReadAsStringAsync(cancellationToken);

        await using var stream =
            await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);
        var body = new StringBuilder();

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            body.AppendLine(line);
            if (line.Trim().Equals("data: [DONE]", StringComparison.Ordinal))
                break;
        }

        return body.ToString();
    }

    private static string NormalizeOptionalFields(string responseText, int requestNumber)
    {
        var node = JsonNode.Parse(responseText) as JsonObject
            ?? throw new JsonException("Vault LLM response must be a JSON object.");
        if (requestNumber is <= 3 && !node.ContainsKey("warnings"))
            node["warnings"] = new JsonArray();
        return node.ToJsonString();
    }

    private static string ProviderDetail(string responseText)
    {
        try
        {
            using var document = JsonDocument.Parse(responseText);
            var message = document.RootElement
                .GetProperty("error")
                .GetProperty("message")
                .GetString();
            return string.IsNullOrWhiteSpace(message)
                ? string.Empty
                : $" {message[..Math.Min(message.Length, 500)]}";
        }
        catch
        {
            return string.Empty;
        }
    }
}
