using AutoX.Gara.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using Nalix.Framework.Configuration;
using Nalix.Framework.Injection;
using System;
using System.Net.NetworkInformation;
namespace AutoX.Gara.Infrastructure.Database;
/// <summary>
/// Factory dung cho design-time de tao <see cref="AutoXDbContext"/>.
/// </summary>
public sealed class AutoXDbContextFactory : IDesignTimeDbContextFactory<AutoXDbContext>
{
    private static readonly DbContextOptionsBuilder<AutoXDbContext> OptionsBuilder = new();
    static AutoXDbContextFactory()
    {
        ILogger logger = InstanceManager.Instance.GetExistingInstance<ILogger>();
        logger?.Debug($"[DB.{nameof(AutoXDbContextFactory)}] Start initialization sequence.");
        DatabaseOptions configuration = ConfigurationManager.Instance.Get<DatabaseOptions>();
        // Kiem tra ket noi den database
        if (!configuration.DatabaseType.Equals("SQLite", StringComparison.OrdinalIgnoreCase) &&
            !CAN_CONNECT_TO_DATABASE(configuration.ConnectionString))
        {
            logger?.Error($"[DB.{nameof(AutoXDbContextFactory)}] Cannot connect to the database at {configuration.ConnectionString}");
            throw new InvalidOperationException($"Cannot connect to the database at {configuration.ConnectionString}");
        }
        try
        {
            if (configuration.DatabaseType.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                OptionsBuilder.UseNpgsql(configuration.ConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    npgsqlOptions.CommandTimeout(60);
                    npgsqlOptions.MigrationsHistoryTable("__MigrationsHistory", "public");
                    npgsqlOptions.UseRelationalNulls();
                    npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
                .EnableSensitiveDataLogging(false)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableDetailedErrors(false);
            }
            else if (configuration.DatabaseType.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
            {
                OptionsBuilder.UseSqlite(configuration.ConnectionString, sqliteOptions =>
                {
                    sqliteOptions.CommandTimeout(60);
                    sqliteOptions.MigrationsHistoryTable("__MigrationsHistory");
                })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported database type: {configuration.DatabaseType}");
            }
        }
        catch (Exception ex)
        {
            logger?.Error($"[DB.{nameof(AutoXDbContextFactory)}] Error configuring DbContext.", ex);
            throw;
        }
    }
    public AutoXDbContext CreateDbContext(string[] args = null) => new(OptionsBuilder.Options);
    private static bool CAN_CONNECT_TO_DATABASE(string connectionString)
    {
        try
        {
            Npgsql.NpgsqlConnectionStringBuilder builder = new(connectionString);
            string host = builder.Host;
            using Ping ping = new();
            PingReply reply = ping.Send(host, 3000);
            return reply.Status == IPStatus.Success;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
