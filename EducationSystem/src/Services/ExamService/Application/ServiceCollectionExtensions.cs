using ExamService.Application.Interfaces;
using ExamService.Application.Services;

namespace ExamService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IQuestionSuiteQueryService, QuestionSuiteQueryService>();
        services.AddScoped<IQuestionQueryService, QuestionQueryService>();
        services.AddScoped<IQuestionAnswerQueryService, QuestionAnswerQueryService>();
        services.AddScoped<ISubjectTeachingExamQueryService, SubjectTeachingExamQueryService>();
        services.AddScoped<IExamAttemptQueryService, ExamAttemptQueryService>();
        services.AddScoped<IExamResultQueryService, ExamResultQueryService>();
        services.AddScoped<IExamQuestionSelectionQueryService, ExamQuestionSelectionQueryService>();
        services.AddScoped<IExamQuestionAnswerQueryService, ExamQuestionAnswerQueryService>();
        return services;
    }
}
