using CareerService.Application;
using Microsoft.Extensions.Options;

namespace CareerService.Infrastructure;

public sealed class GeminiAiOutcomeMappingProvider(
    HttpClient httpClient,
    IOptions<PdfOcrOptions> options,
    ILoggerFactory loggerFactory) : IAiOutcomeMappingProvider
{
    public Task<OutcomeExtractionProviderResult> MapAsync(
        OutcomeExtractionProviderRequest request,
        CancellationToken cancellationToken)
    {
        if (request.RequestNumber != 4)
            throw new OutcomeImportException(
                "AI_MAPPING_TASK_NOT_ALLOWED",
                "The Gemini mapping provider only accepts the CLO-PLO mapping task.",
                StatusCodes.Status400BadRequest);

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey) ||
            string.IsNullOrWhiteSpace(settings.Model))
            throw new OutcomeImportException(
                "AI_MAPPING_NOT_CONFIGURED",
                "AI mapping requires PdfOcr:Model and PdfOcr:ApiKey configuration.",
                StatusCodes.Status503ServiceUnavailable);

        var provider = new GeminiOutcomeExtractionProvider(
            httpClient,
            Options.Create(new LlmOptions
            {
                Provider = "Gemini",
                Model = settings.Model,
                ApiKey = settings.ApiKey,
                TimeoutSeconds = settings.TimeoutSeconds,
                MaxRetries = settings.MaxRetries,
                MaxInputTokensPerRequest = 12_000,
                PromptVersion = request.PromptVersion
            }),
            loggerFactory.CreateLogger<GeminiOutcomeExtractionProvider>());
        return provider.ExtractAsync(request, cancellationToken);
    }
}
