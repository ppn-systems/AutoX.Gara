// Sample log message changes only - logic and structure unchanged

using AutoX.Gara.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nalix.Common.Diagnostics;
using Nalix.Common.Environment;
using Nalix.Framework.Configuration;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Infrastructure.Database;

public class AutoDbContextFactory : IDesignTimeDbContextFactory<AutoDbContext>
{
    public AutoDbContext CreateDbContext(System.String[] args)
    {
        InstanceManager.Instance.GetExistingInstance<ILogger>()?
            .Info($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CreateDbContext)}] Start initialization sequence.");

        // Load cấu hình từ DatabaseOptions
        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Debug($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CreateDbContext)}] Loading DatabaseOptions...");
        DatabaseOptions configuration = ConfigurationManager.Instance.Get<DatabaseOptions>();

        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Debug($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CreateDbContext)}] DatabaseType = {configuration.DatabaseType}, ConnectionString = {configuration.ConnectionString}");

        // Kiểm tra kết nối đến database
        if (!configuration.DatabaseType.Equals("SQLite", System.StringComparison.OrdinalIgnoreCase) &&
            !CanConnectToDatabase(configuration.ConnectionString))
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CreateDbContext)}] Cannot connect to the database at {configuration.ConnectionString}");
            throw new System.InvalidOperationException($"Cannot connect to the database at {configuration.ConnectionString}");
        }

        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Debug($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CreateDbContext)}] Database connectivity check passed.");

        DbContextOptionsBuilder<AutoDbContext> optionsBuilder = new();

        try
        {
            if (configuration.DatabaseType.Equals("PostgreSQL", System.StringComparison.OrdinalIgnoreCase))
            {
                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                    .Debug($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CreateDbContext)}] Configuring DbContext for PostgreSQL.");

                optionsBuilder.UseNpgsql(configuration.ConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(5, System.TimeSpan.FromSeconds(30), null);
                    npgsqlOptions.CommandTimeout(60);
                    npgsqlOptions.MigrationsHistoryTable("__MigrationsHistory", "public");
                    npgsqlOptions.UseRelationalNulls();
                    npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                })
                .EnableSensitiveDataLogging(false)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableDetailedErrors(false)
                .EnableServiceProviderCaching();

                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                        .Info($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CreateDbContext)}] DbContext configured for PostgreSQL.");
            }
            else if (configuration.DatabaseType.Equals("SQLite", System.StringComparison.OrdinalIgnoreCase))
            {
                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                        .Debug($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CreateDbContext)}] Configuring DbContext for SQLite.");

                optionsBuilder.UseSqlite(
                    $"Data Source={Directories.DataDirectory}\\Auto.db",
                    sqliteOptions =>
                    {
                        sqliteOptions.CommandTimeout(60);
                        sqliteOptions.MigrationsHistoryTable("__MigrationsHistory");
                    })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableServiceProviderCaching();

                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                        .Info($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CreateDbContext)}] DbContext configured for SQLite.");
            }
            else
            {
                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                        .Warn($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CreateDbContext)}] Unsupported database type: {configuration.DatabaseType}");

                throw new System.InvalidOperationException($"Unsupported database type: {configuration.DatabaseType}");
            }
        }
        catch (System.Exception ex)
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CreateDbContext)}] Error configuring DbContext for {configuration.DatabaseType}.", ex);
            throw;
        }

        AutoDbContext dbContext = new(optionsBuilder.Options);

        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Info($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CreateDbContext)}] AutoDbContext successfully created.");

        return dbContext;
    }

    private static System.Boolean CanConnectToDatabase(System.String connectionString)
    {
        try
        {
            Npgsql.NpgsqlConnectionStringBuilder builder = new(connectionString);

            System.String host = builder.Host;
            System.Int32 port = builder.Port;

            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Info($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CanConnectToDatabase)}] Pinging database server {host}:{port}...");

            using System.Net.NetworkInformation.Ping ping = new();
            System.Net.NetworkInformation.PingReply reply = ping.Send(host, 3000); // Timeout 1 giây

            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                        .Info($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CanConnectToDatabase)}] Ping to {host} successful.");
                return true;
            }

            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CanConnectToDatabase)}] Ping to {host} failed: {reply.Status}");
            return false;
        }
        catch (System.Exception ex)
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[DB.{nameof(AutoDbContextFactory)}:{nameof(CanConnectToDatabase)}] Error pinging database server.", ex);
            return false;
        }
    }
}