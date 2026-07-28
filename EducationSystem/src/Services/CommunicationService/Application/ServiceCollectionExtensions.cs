using CommunicationService.Application.Interfaces;
using CommunicationService.Application.Options;
using CommunicationService.Application.Services;

namespace CommunicationService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IFormTemplateQueryService, FormTemplateQueryService>();
        services.AddScoped<IFormRequestQueryService, FormRequestQueryService>();
        services.AddScoped<IStudentFormRequestService, StudentFormRequestService>();
        services.AddScoped<IStudentAnnouncementService, StudentAnnouncementService>();
        services.AddScoped<IUserAnnouncementQueryService, UserAnnouncementQueryService>();
        services.AddScoped<IManagementAnnouncementReadService, ManagementAnnouncementReadService>();
        services.AddScoped<IInternshipVerificationService, InternshipVerificationService>();
        services.AddScoped<IAcademicInternshipClient, AcademicInternshipClient>();
        services.AddOptions<InternshipEmailOptions>()
            .BindConfiguration(InternshipEmailOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(
                options => Uri.TryCreate(options.PortalBaseUrl, UriKind.Absolute, out var uri) &&
                    (uri.Scheme == Uri.UriSchemeHttps || uri.IsLoopback),
                "InternshipEmail:PortalBaseUrl must be an absolute HTTPS URL (HTTP is allowed only for localhost).")
            .Validate(
                options => string.IsNullOrWhiteSpace(options.Username) ||
                    !string.IsNullOrWhiteSpace(options.Password),
                "InternshipEmail:Password is required when Username is configured.")
            .ValidateOnStart();
        services.AddSingleton<IInternshipVerificationEmailSender, SmtpInternshipVerificationEmailSender>();
        services.AddSingleton(TimeProvider.System);
        services.AddHttpClient("AcademicService", (serviceProvider, client) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri(
                configuration["ServiceEndpoints:AcademicBaseUrl"] ?? "http://localhost:5002");
            client.Timeout = TimeSpan.FromSeconds(10);

            var apiKey = configuration["InternalApi:Key"];
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);
            }
        });
        return services;
    }
}
