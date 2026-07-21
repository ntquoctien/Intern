using CommunicationService.Application.Interfaces;
using CommunicationService.Application.Services;

namespace CommunicationService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IFormTemplateQueryService, FormTemplateQueryService>();
        services.AddScoped<IFormRequestQueryService, FormRequestQueryService>();
        services.AddScoped<IStudentFormRequestService, StudentFormRequestService>();
        services.AddScoped<IUserAnnouncementQueryService, UserAnnouncementQueryService>();
        return services;
    }
}
