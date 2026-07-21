using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using IdentityService.Application.StudentAccess;
using Microsoft.Extensions.Options;

namespace IdentityService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserQueryService, UserQueryService>();
        services.AddScoped<IUserDeviceQueryService, UserDeviceQueryService>();
        services.AddScoped<IPasswordResetQueryService, PasswordResetQueryService>();
        services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();
        services.AddScoped<ISettingQueryService, SettingQueryService>();
        services.AddScoped<IStudentLoginService, StudentLoginService>();
        services.AddHttpClient<IAcademicStudentIdentityClient, AcademicStudentIdentityClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<StudentLoginOptions>>().Value;
            client.BaseAddress = new Uri(options.AcademicServiceBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.AcademicLookupTimeoutSeconds, 1, 10));
        });
        return services;
    }
}
