using CareerService.Application;
using CareerService.Infrastructure;
using CareerService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

AddSharedConnectionStringFile(builder);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
builder.Services.AddOptions<ManagementAuthOptions>()
    .Bind(builder.Configuration.GetSection(ManagementAuthOptions.SectionName));
builder.Services.AddOptions<AcademicClientOptions>()
    .Bind(builder.Configuration.GetSection(AcademicClientOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("CareerDb")
    ?? throw new InvalidOperationException("Connection string 'CareerDb' was not found.");
builder.Services.AddDbContext<CareerDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddAuthentication("Management")
    .AddScheme<AuthenticationSchemeOptions, DevelopmentManagementAuthenticationHandler>(
        "Management", _ => { });
builder.Services.AddAuthorization(options =>
    options.AddPolicy(
        "SystemAdministrator",
        policy => policy.RequireAuthenticatedUser().RequireRole("SystemAdministrator")));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IOutcomeFileStorage, LocalOutcomeFileStorage>();
builder.Services.AddScoped<IOutcomeDocumentParser, OpenXmlOutcomeDocumentParser>();
builder.Services.AddScoped<DocxFileValidator>();
builder.Services.AddScoped<OutcomeReconciliationService>();
builder.Services.AddScoped<IOutcomeValidationService, OutcomeValidationService>();
builder.Services.AddScoped<OutcomeQualityEvaluator>();
builder.Services.AddScoped<ApprovedOutcomeDocumentService>();
builder.Services.AddScoped<OutcomeImportService>();
builder.Services.AddScoped<IOutcomeImportProcessor, OutcomeImportProcessor>();
builder.Services.AddHostedService<OutcomeImportWorker>();
var llmProvider = builder.Configuration[$"{LlmOptions.SectionName}:Provider"] ?? "Gemini";
if (string.Equals(llmProvider, "Groq", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddHttpClient<IOutcomeExtractionProvider, GroqOutcomeExtractionProvider>(
        ConfigureLlmClient);
else if (string.Equals(llmProvider, "Gemini", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddHttpClient<IOutcomeExtractionProvider, GeminiOutcomeExtractionProvider>(
        ConfigureLlmClient);
else
    throw new InvalidOperationException(
        $"Unsupported LLM provider '{llmProvider}'. Use Gemini or Groq.");
builder.Services.AddHttpClient<AcademicSubjectClient>();

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

public partial class Program;
