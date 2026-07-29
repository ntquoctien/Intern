using CareerService.Application;
using Microsoft.AspNetCore.Mvc;

namespace CareerService.Infrastructure;

public sealed class OutcomeExceptionMiddleware(
    RequestDelegate next,
    ILogger<OutcomeExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OutcomeImportException exception)
        {
            await WriteProblem(context, exception.StatusCode, exception.ErrorCode, exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled CareerService error.");
            await WriteProblem(
                context, StatusCodes.Status500InternalServerError,
                "UNEXPECTED_ERROR", "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblem(
        HttpContext context,
        int status,
        string errorCode,
        string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = status,
            Title = errorCode,
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["errorCode"] = errorCode;
        problem.Extensions["traceId"] = context.TraceIdentifier;
        await context.Response.WriteAsJsonAsync(problem);
    }
}
