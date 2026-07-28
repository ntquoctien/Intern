using AcademicService.Application.Interfaces;
using AcademicService.Application.Services;

namespace AcademicService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IStudentQueryService, StudentQueryService>();
        services.AddScoped<IStudentIdentityResolver, StudentIdentityResolver>();
        services.AddScoped<IStudentSelfService, StudentSelfService>();
        services.AddScoped<IManagementReadService, ManagementReadService>();
        services.AddHttpClient<IIdentityUserClient, IdentityUserClient>((serviceProvider, client) =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri(
                configuration["ServiceEndpoints:IdentityBaseUrl"] ?? "http://localhost:5001");
            client.Timeout = TimeSpan.FromSeconds(3);
        });
        services.AddScoped<IFacultyQueryService, FacultyQueryService>();
        services.AddScoped<IMajorQueryService, MajorQueryService>();
        services.AddScoped<IAcademicYearQueryService, AcademicYearQueryService>();
        services.AddScoped<IRoomQueryService, RoomQueryService>();
        services.AddScoped<ISubjectQueryService, SubjectQueryService>();
        services.AddScoped<ISubjectTeachingQueryService, SubjectTeachingQueryService>();
        services.AddScoped<ISubjectStudentQueryService, SubjectStudentQueryService>();
        services.AddScoped<ISubjectScheduleQueryService, SubjectScheduleQueryService>();
        services.AddScoped<IAttendanceQueryService, AttendanceQueryService>();
        services.AddScoped<ISemesterTuitionQueryService, SemesterTuitionQueryService>();
        services.AddScoped<IEvaluationCriteriaQueryService, EvaluationCriteriaQueryService>();
        services.AddScoped<IStudentEvaluationQueryService, StudentEvaluationQueryService>();
        services.AddScoped<IResumeDataService, ResumeDataService>();
        services.AddScoped<IStudentInternshipService, StudentInternshipService>();
        return services;
    }
}
