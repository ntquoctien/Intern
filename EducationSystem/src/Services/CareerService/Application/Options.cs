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
}
