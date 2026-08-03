using CareerService.Application;
using CareerService.Infrastructure;
using CareerService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

var builder = WebApplication.CreateBuilder(args);

AddSharedConnectionStringFile(builder);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name));
builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                && (uri.Host == "localhost" || uri.Host == "127.0.0.1"))
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddOptions<OutcomeStorageOptions>()
    .Bind(builder.Configuration.GetSection(OutcomeStorageOptions.SectionName))
    .Validate(options => options.MaxFileSizeBytes > 0, "OutcomeStorage max file size must be positive.")
    .ValidateOnStart();
builder.Services.AddOptions<LlmOptions>()
    .Bind(builder.Configuration.GetSection(LlmOptions.SectionName));
builder.Services.PostConfigure<LlmOptions>(settings =>
{
    if (!settings.Provider.Equals("Vault", StringComparison.OrdinalIgnoreCase))
        return;
    settings.Model = FirstConfigured(
        builder.Configuration[$"{VaultLlmOptions.SectionName}:Model"],
        settings.Model);
    settings.ApiKey = FirstConfigured(
        builder.Configuration[$"{VaultLlmOptions.SectionName}:ApiKey"],
        settings.ApiKey);
});
builder.Services.AddOptions<PdfOcrOptions>()
    .Bind(builder.Configuration.GetSection(PdfOcrOptions.SectionName))
    .Validate(options => !options.Enabled ||
        options.Provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase) ||
        options.Provider.Equals("Vault", StringComparison.OrdinalIgnoreCase),
        "PdfOcr provider must be Gemini or Vault.")
    .Validate(options => options.TimeoutSeconds is >= 5 and <= 600,
        "PdfOcr timeout must be between 5 and 600 seconds.")
    .Validate(options => options.MaxRetries is >= 0 and <= 5,
        "PdfOcr retries must be between 0 and 5.")
    .Validate(options => options.MaxOutputTokens is >= 1024 and <= 65536,
        "PdfOcr max output tokens must be between 1024 and 65536.")
    .ValidateOnStart();
builder.Services.AddOptions<VaultLlmOptions>()
    .Bind(builder.Configuration.GetSection(VaultLlmOptions.SectionName))
    .Validate(
        options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
        "VaultLLM:BaseUrl must be an absolute URI.")
    .Validate(options => options.TimeoutSeconds is >= 5 and <= 600,
        "VaultLLM timeout must be between 5 and 600 seconds.")
    .Validate(options => options.MaxRetries is >= 0 and <= 5,
        "VaultLLM retries must be between 0 and 5.")
    .Validate(options => options.MaxOutputTokens is >= 512 and <= 32768,
        "VaultLLM max output tokens must be between 512 and 32768.")
    .ValidateOnStart();
builder.Services.AddOptions<ResumeLlmOptions>()
    .Bind(builder.Configuration.GetSection(ResumeLlmOptions.SectionName));
builder.Services.PostConfigure<ResumeLlmOptions>(settings =>
{
    settings.Provider = FirstConfigured(
        settings.Provider,
        builder.Configuration[$"{LlmOptions.SectionName}:Provider"],
        "Gemini");
    settings.Model = FirstConfigured(
        settings.Model,
        builder.Configuration[$"{LlmOptions.SectionName}:Model"]);
    settings.ApiKey = FirstConfigured(
        settings.ApiKey,
        builder.Configuration[$"{LlmOptions.SectionName}:ApiKey"]);
    if (settings.TimeoutSeconds <= 0)
        settings.TimeoutSeconds =
            builder.Configuration.GetValue<int?>(
                $"{LlmOptions.SectionName}:TimeoutSeconds") ?? 180;
    if (settings.MaxRetries < 0)
        settings.MaxRetries =
            builder.Configuration.GetValue<int?>(
                $"{LlmOptions.SectionName}:MaxRetries") ?? 2;
    if (settings.MaxInputTokensPerRequest <= 0)
        settings.MaxInputTokensPerRequest =
            builder.Configuration.GetValue<int?>(
                $"{LlmOptions.SectionName}:MaxInputTokensPerRequest") ?? 12_000;
    if (settings.Provider.Equals("Vault", StringComparison.OrdinalIgnoreCase))
    {
        settings.BaseUrl = FirstConfigured(
            settings.BaseUrl,
            builder.Configuration[$"{VaultLlmOptions.SectionName}:BaseUrl"]);
        settings.Model = FirstConfigured(
            builder.Configuration[$"{VaultLlmOptions.SectionName}:Model"],
            settings.Model);
        settings.ApiKey = FirstConfigured(
            builder.Configuration[$"{VaultLlmOptions.SectionName}:ApiKey"],
            settings.ApiKey);
    }
});
builder.Services.AddOptions<ManagementAuthOptions>()
    .Bind(builder.Configuration.GetSection(ManagementAuthOptions.SectionName));
builder.Services.AddOptions<AcademicClientOptions>()
    .Bind(builder.Configuration.GetSection(AcademicClientOptions.SectionName));
builder.Services.AddOptions<VectorMatchClientOptions>()
    .Bind(builder.Configuration.GetSection(VectorMatchClientOptions.SectionName))
    .Validate(
        options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
        "VectorMatchClient:BaseUrl must be an absolute URI.")
    .Validate(
        options => options.TimeoutSeconds is >= 5 and <= 300,
        "VectorMatchClient:TimeoutSeconds must be between 5 and 300.")
    .ValidateOnStart();

var connectionString = builder.Configuration.GetConnectionString("CareerDb")
    ?? throw new InvalidOperationException("Connection string 'CareerDb' was not found.");
builder.Services.AddDbContext<CareerDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddStudentJwtAuthentication(builder.Configuration);
builder.Services.AddAuthentication("Management")
    .AddScheme<AuthenticationSchemeOptions, DevelopmentManagementAuthenticationHandler>(
        "Management", _ => { });
builder.Services.AddAuthorization(options =>
    options.AddPolicy(
        "SystemAdministrator",
        policy => policy
            .AddAuthenticationSchemes("Management")
            .RequireAuthenticatedUser()
            .RequireRole("SystemAdministrator")));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IOutcomeFileStorage, LocalOutcomeFileStorage>();
builder.Services.AddScoped<DocxFileValidator>();
builder.Services.AddScoped<PdfFileValidator>();
builder.Services.AddScoped<OutcomeDocumentFileValidator>();
builder.Services.AddScoped<OpenXmlOutcomeDocumentParser>();
builder.Services.AddScoped<PdfOutcomeDocumentParser>();
var pdfOcrProvider =
    builder.Configuration[$"{PdfOcrOptions.SectionName}:Provider"] ?? "Gemini";
if (pdfOcrProvider.Equals("Vault", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddHttpClient<IPdfOcrService, VaultPdfOcrService>(
        ConfigureVaultPdfOcrClient);
else
    builder.Services.AddHttpClient<IPdfOcrService, GeminiPdfOcrService>(
        ConfigurePdfOcrClient);
builder.Services.AddScoped<IOutcomeDocumentParser, OutcomeDocumentParser>();
builder.Services.AddScoped<OutcomeReconciliationService>();
builder.Services.AddScoped<IOutcomeValidationService, OutcomeValidationService>();
builder.Services.AddScoped<OutcomeQualityEvaluator>();
builder.Services.AddScoped<ApprovedOutcomeDocumentService>();
builder.Services.AddScoped<OutcomeImportService>();
builder.Services.AddScoped<IOutcomeImportProcessor, OutcomeImportProcessor>();
builder.Services.AddScoped<IResumeContextHydrationService, ResumeContextHydrationService>();
builder.Services.AddHostedService<OutcomeImportWorker>();
var llmProvider = builder.Configuration[$"{LlmOptions.SectionName}:Provider"] ?? "Gemini";
if (string.Equals(llmProvider, "Vault", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<VaultOutcomeProvider>(ConfigureVaultLlmClient);
    builder.Services.AddTransient<IOutcomeExtractionProvider>(
        services => services.GetRequiredService<VaultOutcomeProvider>());
    builder.Services.AddTransient<IAiOutcomeMappingProvider>(
        services => services.GetRequiredService<VaultOutcomeProvider>());
}
else if (string.Equals(llmProvider, "Groq", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IOutcomeExtractionProvider, GroqOutcomeExtractionProvider>(
        ConfigureLlmClient);
    builder.Services.AddHttpClient<IAiOutcomeMappingProvider, GeminiAiOutcomeMappingProvider>(
        ConfigurePdfOcrClient);
}
else if (string.Equals(llmProvider, "Gemini", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IOutcomeExtractionProvider, GeminiOutcomeExtractionProvider>(
        ConfigureLlmClient);
    builder.Services.AddHttpClient<IAiOutcomeMappingProvider, GeminiAiOutcomeMappingProvider>(
        ConfigurePdfOcrClient);
}
else
    throw new InvalidOperationException(
        $"Unsupported LLM provider '{llmProvider}'. Use Vault, Gemini, or Groq.");
builder.Services.AddHttpClient<ILlmResumeGeneratorService, LlmResumeGeneratorService>(
    ConfigureResumeLlmClient);
builder.Services.AddHttpClient<AcademicSubjectClient>();
builder.Services.AddHttpClient<IAcademicResumeClient, AcademicResumeClient>(
    (services, client) =>
    {
        var options = services.GetRequiredService<
            Microsoft.Extensions.Options.IOptions<AcademicClientOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(15);
    });
builder.Services.AddHttpClient<IVectorMatchClient, VectorMatchClient>(
    (services, client) =>
    {
        var options = services.GetRequiredService<
            Microsoft.Extensions.Options.IOptions<VectorMatchClientOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    });

var app = builder.Build();

app.UseMiddleware<OutcomeExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseCors("FrontendDev");
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapControllers();
app.Run();

static void AddSharedConnectionStringFile(WebApplicationBuilder builder)
{
    var directory = new DirectoryInfo(builder.Environment.ContentRootPath);
    while (directory is not null &&
           !File.Exists(Path.Combine(directory.FullName, "EducationSystem.sln")))
        directory = directory.Parent;
    if (directory is null) return;
    builder.Configuration.AddJsonFile(
        Path.Combine(
            directory.FullName,
            $"connectionstrings.{builder.Environment.EnvironmentName}.json"),
        optional: true,
        reloadOnChange: true);
}

static void ConfigureLlmClient(IServiceProvider services, HttpClient client)
{
    var options = services.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<LlmOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 300));
}

static void ConfigureResumeLlmClient(IServiceProvider services, HttpClient client)
{
    var options = services.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<ResumeLlmOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 300));
}

static void ConfigurePdfOcrClient(IServiceProvider services, HttpClient client)
{
    var options = services.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<PdfOcrOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 300));
}

static void ConfigureVaultLlmClient(IServiceProvider services, HttpClient client)
{
    var options = services.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<VaultLlmOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 600));
}

static void ConfigureVaultPdfOcrClient(IServiceProvider services, HttpClient client)
{
    var options = services.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<PdfOcrOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 5, 600));
}

static string FirstConfigured(params string?[] values) =>
    values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

public partial class Program;
