using CareerService.Application;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

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
            await WriteProblem(
                context,
                exception.StatusCode,
                exception.ErrorCode,
                exception.Message,
                exception.Extensions);
        }
        catch (DownstreamApiException exception)
        {
            await WriteProblem(context, exception.StatusCode, exception.ErrorCode, exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            await WriteProblem(
                context,
                StatusCodes.Status401Unauthorized,
                StudentErrorCodes.Unauthorized,
                exception.Message);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation(
                "CareerService request {Method} {Path} was canceled by the client.",
                context.Request.Method,
                context.Request.Path);
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
        string detail,
        IReadOnlyDictionary<string, object?>? extensions = null)
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
        if (extensions is not null)
            foreach (var extension in extensions)
                problem.Extensions[extension.Key] = extension.Value;
        await context.Response.WriteAsJsonAsync(problem);
    }
}
