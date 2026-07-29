using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using CareerService.Application;
using Microsoft.Extensions.Options;

namespace CareerService.Infrastructure;

public sealed class GroqOutcomeExtractionProvider(
    HttpClient httpClient,
    IOptions<LlmOptions> options,
    ILogger<GroqOutcomeExtractionProvider> logger) : IOutcomeExtractionProvider
{
    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";

    public async Task<OutcomeExtractionProviderResult> ExtractAsync(
        OutcomeExtractionProviderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.RequestNumber is < 1 or > 3)
            throw new OutcomeImportException(
                "LLM_TASK_NOT_ALLOWED",
                "Only the three configured outcome-import extraction tasks may invoke the LLM.", 400);

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey) || string.IsNullOrWhiteSpace(settings.Model))
            throw new OutcomeImportException(
                "LLM_NOT_CONFIGURED",
                "Groq model and API key must be configured before processing.", 503);
        if (request.InputJson.Length > settings.MaxInputTokensPerRequest * 4L)
            throw new OutcomeImportException(
                "LLM_INPUT_TOO_LARGE",
                "The extracted document exceeds the configured per-request input limit.", 422);

        var responseSchema = GeminiOutcomeExtractionProvider
            .BuildResponseSchema(request.RequestNumber)
            .ToJsonString();
        var prompt =
            $"""
             {GeminiOutcomeExtractionProvider.BuildPrompt(request)}

             Return one JSON object matching this schema. Do not wrap it in markdown:
             {responseSchema}
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
            reasoning_effort = "none",
            response_format = new { type = "json_object" }
        };

        Exception? last = null;
        for (var attempt = 0; attempt <= settings.MaxRetries; attempt++)
        {
            try
            {
                using var message = new HttpRequestMessage(HttpMethod.Post, Endpoint)
                {
                    Content = JsonContent.Create(payload)
                };
                message.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", settings.ApiKey);
                using var response = await httpClient.SendAsync(message, cancellationToken);
                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    if (IsTransient(response.StatusCode) && attempt < settings.MaxRetries)
                    {
                        await Task.Delay(
                            RetryDelay(response, responseText, attempt),
                            cancellationToken);
                        continue;
                    }
                    var providerDetail = ReadProviderError(responseText);
                    throw new OutcomeImportException(
                        response.StatusCode switch
                        {
                            HttpStatusCode.TooManyRequests => "LLM_RATE_LIMIT",
                            HttpStatusCode.RequestEntityTooLarge => "LLM_INPUT_TOO_LARGE",
                            _ => "LLM_REQUEST_FAILED"
                        },
                        $"Groq request failed with HTTP {(int)response.StatusCode}.{providerDetail}",
                        response.StatusCode == HttpStatusCode.RequestEntityTooLarge ? 422 : 502);
                }

                using var parsed = JsonDocument.Parse(responseText);
                var text = parsed.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
                if (string.IsNullOrWhiteSpace(text))
                    throw new JsonException("Groq response content is empty.");
                var normalizedText = NormalizeOptionalFields(text);
                using var structured = JsonDocument.Parse(normalizedText);
                GeminiOutcomeExtractionProvider.EnsureExpectedShape(
                    structured.RootElement, request.RequestNumber);
                var requestId = response.Headers.TryGetValues("x-request-id", out var values)
                    ? values.FirstOrDefault()
                    : null;
                return new OutcomeExtractionProviderResult(
                    structured.RootElement.GetRawText(), requestId);
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

        logger.LogWarning(
            last,
            "Groq extraction failed for restricted request {RequestNumber}",
            request.RequestNumber);
        throw new OutcomeImportException(
            "LLM_INVALID_RESPONSE",
            "Groq did not return valid structured JSON.", 502, last);
    }

    private static bool IsTransient(HttpStatusCode status) =>
        status == HttpStatusCode.TooManyRequests || (int)status >= 500;

    private static string NormalizeOptionalFields(string responseText)
    {
        var node = JsonNode.Parse(responseText) as JsonObject
            ?? throw new JsonException("Groq response must be a JSON object.");
        if (!node.ContainsKey("warnings"))
            node["warnings"] = new JsonArray();
        return node.ToJsonString();
    }

    private static TimeSpan RetryDelay(
        HttpResponseMessage response,
        string responseText,
        int attempt)
    {
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            if (response.Headers.RetryAfter?.Delta is { } delta)
                return TimeSpan.FromSeconds(Math.Clamp(delta.TotalSeconds, 1, 60));
            var detail = ReadProviderError(responseText);
            var match = Regex.Match(
                detail,
                @"try again in\s+([0-9]+(?:\.[0-9]+)?)s",
                RegexOptions.IgnoreCase);
            if (match.Success &&
                double.TryParse(
                    match.Groups[1].Value,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var seconds))
                return TimeSpan.FromSeconds(Math.Clamp(Math.Ceiling(seconds) + 1, 1, 60));
        }
        return TimeSpan.FromSeconds(Math.Pow(2, attempt + 1));
    }

    private static string ReadProviderError(string responseText)
    {
        try
        {
            using var document = JsonDocument.Parse(responseText);
            if (!document.RootElement.TryGetProperty("error", out var error) ||
                !error.TryGetProperty("message", out var message))
                return string.Empty;
            var value = message.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            value = Regex.Replace(
                value,
                @"\s+in organization\s+`\S+`",
                string.Empty,
                RegexOptions.IgnoreCase);
            value = Regex.Replace(
                value,
                @"\s+Need more tokens\?.*$",
                string.Empty,
                RegexOptions.IgnoreCase);
            return $" {value[..Math.Min(value.Length, 500)]}";
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }
}
