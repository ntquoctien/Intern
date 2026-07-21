using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SharedKernel;

public sealed class ReadOnlyCommandInterceptor : DbCommandInterceptor
{
    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result) => Reject<int>(command);

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(Reject<int>(command));

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        EnsureReadCommand(command);
        return result;
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        EnsureReadCommand(command);
        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        EnsureReadCommand(command);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        EnsureReadCommand(command);
        return ValueTask.FromResult(result);
    }

    private static InterceptionResult<T> Reject<T>(DbCommand command)
    {
        throw new InvalidOperationException($"Database mutation is disabled for this read-only application. Command type: {command.CommandType}.");
    }

    private static void EnsureReadCommand(DbCommand command)
    {
        var sql = command.CommandText.TrimStart();
        if (!sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) &&
            !sql.StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only SELECT/CTE database commands are allowed by the read-only runtime guard.");
        }
    }
}
