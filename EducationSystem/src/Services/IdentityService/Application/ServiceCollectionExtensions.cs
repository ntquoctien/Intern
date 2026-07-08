using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;

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
        return services;
    }
}
