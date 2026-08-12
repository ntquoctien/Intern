using System.Data;
using AcademicService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AcademicService.Infrastructure;

/// <summary>
/// Management screens are read-only reports. Let them read the latest
/// committed/working snapshot without waiting behind long-running seed or
/// import transactions.
/// </summary>
public sealed class ManagementReadIsolationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AcademicDbContext dbContext)
    {
        if (!HttpMethods.IsGet(context.Request.Method) ||
            !context.Request.Path.StartsWithSegments("/api/management"))
        {
            await next(context);
            return;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadUncommitted,
            context.RequestAborted);
        await next(context);
        await transaction.CommitAsync(context.RequestAborted);
    }
}
