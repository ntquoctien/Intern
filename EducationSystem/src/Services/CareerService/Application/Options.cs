namespace CareerService.Application;

public sealed class OutcomeStorageOptions
{
    public const string SectionName = "OutcomeStorage";
    public string RootPath { get; set; } = string.Empty;
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public long MaxExpandedSizeBytes { get; set; } = 100 * 1024 * 1024;
    public int MaxZipEntries { get; set; } = 5000;
}

public sealed class LlmOptions
{
    public const string SectionName = "LLM";
    public string Provider { get; set; } = "Gemini";
    public string Model { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 180;
    public int MaxRetries { get; set; } = 2;
    public int MaxRequestsPerBatch { get; set; } = 30;
    public int MaxInputTokensPerRequest { get; set; } = 12000;
    public string PromptVersion { get; set; } = "outcome-import-v1";
}

public sealed class ManagementAuthOptions
{
    public const string SectionName = "ManagementAuth";
    public bool AllowDevelopmentBypass { get; set; }
}

public sealed class AcademicClientOptions
{
    public const string SectionName = "AcademicClient";
    public string BaseUrl { get; set; } = "http://localhost:5002";
    public int TimeoutSeconds { get; set; } = 60;
}

public sealed class VaultLlmOptions
{
    public const string SectionName = "VaultLLM";
    public string BaseUrl { get; set; } = "https://newapi.vault.io.vn/v1";
    public string Model { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 300;
    public int MaxRetries { get; set; } = 2;
    public int MaxOutputTokens { get; set; } = 4000;
}

public sealed class ResumeLlmOptions
{
    public const string SectionName = "ResumeLLM";
    public string Provider { get; set; } = "Vault";
    public string Model { get; set; } = "gpt-5.6-sol";
    public string FallbackModel { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://newapi.vault.io.vn/v1";
    public int TimeoutSeconds { get; set; } = 300;
    public int MaxRetries { get; set; } = 2;
    public int MaxInputTokensPerRequest { get; set; } = 12000;
}

public sealed class PdfOcrOptions
{
    public const string SectionName = "PdfOcr";
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "Vault";
    public string Model { get; set; } = "gpt-5.6-sol";
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 600;
    public int MaxRetries { get; set; } = 2;
    public int MaxOutputTokens { get; set; } = 32768;
}

public sealed class VectorMatchClientOptions
{
    public const string SectionName = "VectorMatchClient";
    public string BaseUrl { get; set; } = "http://localhost:5006";
    public string InternalApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 60;
}

