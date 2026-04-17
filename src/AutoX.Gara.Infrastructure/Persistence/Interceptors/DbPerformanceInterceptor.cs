using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AutoX.Gara.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor giám sát hiệu năng truy vấn cơ sở dữ liệu.
/// Log cảnh báo nếu truy vấn vượt quá ngưỡng cho phép (100ms).
/// </summary>
public sealed class DbPerformanceInterceptor(ILogger<DbPerformanceInterceptor> logger) : DbCommandInterceptor
{
    private readonly ILogger _logger = logger;
    private const long SlowQueryThresholdMs = 100;

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, 
        CommandExecutedEventData eventData, 
        DbDataReader result, 
        CancellationToken cancellationToken = default)
    {
        if (eventData.Duration.TotalMilliseconds > SlowQueryThresholdMs)
        {
            _logger.LogWarning("Slow Database Query Detected ({Duration}ms): {Sql}", eventData.Duration.TotalMilliseconds, command.CommandText);
        }
        return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken).ConfigureAwait(false);
    }

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        if (eventData.Duration.TotalMilliseconds > SlowQueryThresholdMs)
        {
            _logger.LogWarning("Slow Database Query Detected ({Duration}ms): {Sql}", eventData.Duration.TotalMilliseconds, command.CommandText);
        }
        return base.ReaderExecuted(command, eventData, result);
    }
}
