using System.Text;
using System.Text.Json;

namespace CareerService.Infrastructure;

internal static class VaultChatResponseParser
{
    public static string ExtractJsonObject(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new JsonException("LLM response content is empty.");

        var normalized = NormalizeContent(content);
        for (var index = normalized.IndexOf('{');
             index >= 0;
             index = normalized.IndexOf('{', index + 1))
        {
            try
            {
                var utf8 = Encoding.UTF8.GetBytes(normalized[index..]);
                var reader = new Utf8JsonReader(utf8);
                using var document = JsonDocument.ParseValue(ref reader);
                if (document.RootElement.ValueKind == JsonValueKind.Object)
                    return document.RootElement.GetRawText();
            }
            catch (JsonException)
            {
                // A prose prefix may contain braces. Try the next object start.
            }
        }

        throw new JsonException("LLM response did not contain a valid JSON object.");
    }

    public static string ExtractResponsesContent(string responseBody)
    {
        if (responseBody.TrimStart().StartsWith("event:", StringComparison.Ordinal) ||
            responseBody.TrimStart().StartsWith("data:", StringComparison.Ordinal))
            return ExtractStreamingResponsesContent(responseBody);

        using var document = JsonDocument.Parse(responseBody);
        return NormalizeContent(ExtractResponsesOutput(document.RootElement));
    }

    private static string ExtractStreamingResponsesContent(string responseBody)
    {
        var deltas = new StringBuilder();
        string? completedContent = null;

        foreach (var rawLine in responseBody.Split('\n'))
        {
            var line = rawLine.Trim();
            if (!line.StartsWith("data:", StringComparison.Ordinal))
                continue;

            var data = line["data:".Length..].Trim();
            if (data.Length == 0 || data == "[DONE]")
                continue;

            using var document = JsonDocument.Parse(data);
            var root = document.RootElement;
            
            // Handle Gemini/Vault format
            if (root.TryGetProperty("type", out var type) &&
                type.GetString() == "response.output_text.delta" &&
                root.TryGetProperty("delta", out var delta) &&
                delta.ValueKind == JsonValueKind.String)
                deltas.Append(delta.GetString());
            
            // Handle Claude/Anthropic format (content_block_delta)
            if (root.TryGetProperty("type", out var claudeType) &&
                claudeType.GetString() == "content_block_delta" &&
                root.TryGetProperty("delta", out var claudeDelta) &&
                claudeDelta.TryGetProperty("text", out var claudeText) &&
                claudeText.ValueKind == JsonValueKind.String)
                deltas.Append(claudeText.GetString());

            if (root.TryGetProperty("response", out var response))
            {
                try
                {
                    completedContent = ExtractResponsesOutput(response);
                }
                catch (JsonException)
                {
                    // Intermediate response events do not contain final output yet.
                }
            }
        }

        var content = deltas.Length > 0 ? deltas.ToString() : completedContent;
        return !string.IsNullOrWhiteSpace(content)
            ? NormalizeContent(content)
            : throw new JsonException("Vault streaming Responses API content is empty.");
    }

    private static string ExtractResponsesOutput(JsonElement response)
    {
        if (!response.TryGetProperty("output", out var output) ||
            output.ValueKind != JsonValueKind.Array)
            throw new JsonException("Vault Responses API output is missing.");

        var content = new StringBuilder();
        foreach (var item in output.EnumerateArray())
        {
            if (!item.TryGetProperty("content", out var parts) ||
                parts.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("text", out var text) &&
                    text.ValueKind == JsonValueKind.String)
                    content.Append(text.GetString());
            }
        }

        return content.Length > 0
            ? content.ToString()
            : throw new JsonException("Vault Responses API content is empty.");
    }

    public static string ExtractContent(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            throw new JsonException("Vault response body is empty.");

        if (!responseBody.TrimStart().StartsWith("data:", StringComparison.Ordinal))
            return NormalizeContent(
                ExtractChunkContent(responseBody)
                ?? throw new JsonException("Vault response content is empty."));

        var content = new StringBuilder();
        foreach (var rawLine in responseBody.Split('\n'))
        {
            var line = rawLine.Trim();
            if (!line.StartsWith("data:", StringComparison.Ordinal))
                continue;

            var data = line["data:".Length..].Trim();
            if (data.Length == 0 || data == "[DONE]")
                continue;

            var chunk = ExtractChunkContent(data);
            if (!string.IsNullOrEmpty(chunk))
                content.Append(chunk);
        }

        return content.Length > 0
            ? NormalizeContent(content.ToString())
            : throw new JsonException("Vault streaming response content is empty.");
    }

    private static string NormalizeContent(string content)
    {
        var trimmed = content.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
            return trimmed;

        var firstLineEnd = trimmed.IndexOf('\n');
        var closingFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        if (firstLineEnd < 0 || closingFence <= firstLineEnd)
            return trimmed;

        return trimmed[(firstLineEnd + 1)..closingFence].Trim();
    }

    private static string? ExtractChunkContent(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!root.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
            return null;

        var choice = choices[0];
        if (choice.TryGetProperty("message", out var message) &&
            message.TryGetProperty("content", out var messageContent))
            return ReadText(messageContent);

        if (choice.TryGetProperty("delta", out var delta) &&
            delta.TryGetProperty("content", out var deltaContent))
            return ReadText(deltaContent);

        return null;
    }

    private static string? ReadText(JsonElement content)
    {
        if (content.ValueKind == JsonValueKind.String)
            return content.GetString();

        if (content.ValueKind != JsonValueKind.Array)
            return null;

        var result = new StringBuilder();
        foreach (var item in content.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
                result.Append(item.GetString());
            else if (item.TryGetProperty("text", out var text) &&
                     text.ValueKind == JsonValueKind.String)
                result.Append(text.GetString());
        }
        return result.Length == 0 ? null : result.ToString();
    }
}
