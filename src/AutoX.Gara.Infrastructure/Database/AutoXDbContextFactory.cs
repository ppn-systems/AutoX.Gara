// Copyright (c) 2026 PPN Corporation. All rights reserved.

using AutoX.Gara.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Nalix.Common.Diagnostics;
using Nalix.Framework.Configuration;
using Nalix.Framework.Injection;

namespace AutoX.Gara.Infrastructure.Database;

/// <summary>
/// Factory dùng cho design-time để tạo <see cref="AutoXDbContext"/>.
/// <para>
/// Class này được Entity Framework Core sử dụng khi:
/// <list type="bullet">
/// <item>Chạy lệnh migration</item>
/// <item>Cập nhật database schema</item>
/// <item>Scaffold DbContext ở design-time</item>
/// </list>
/// </para>
/// </summary>
/// <remarks>
/// Factory này không phụ thuộc vào ASP.NET runtime pipeline
/// và sử dụng cấu hình được cung cấp từ <see cref="DatabaseOptions"/>.
/// </remarks>
public sealed class AutoXDbContextFactory : IDesignTimeDbContextFactory<AutoXDbContext>
{
    /// <summary>
    /// Tạo mới một instance của <see cref="AutoXDbContext"/> tại design-time.
    /// </summary>
    /// <param name="args">
    /// Tham số dòng lệnh được EF Core truyền vào (hiện tại không sử dụng).
    /// </param>
    /// <returns>
    /// Một instance đã được cấu hình hoàn chỉnh của <see cref="AutoXDbContext"/>.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    /// Ném ra khi:
    /// <list type="bullet">
    /// <item>Không thể kết nối tới database</item>
    /// <item>Loại database không được hỗ trợ</item>
    /// </list>
    /// </exception>
    public AutoXDbContext CreateDbContext(System.String[] args)
    {
        InstanceManager.Instance.GetExistingInstance<ILogger>()?
            .Info($"[DB.{nameof(AutoXDbContextFactory)}:{nameof(CreateDbContext)}] Start initialization sequence.");

        // Load cấu hình từ DatabaseOptions
        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Debug($"[DB.{nameof(AutoXDbContextFactory)}:{nameof(CreateDbContext)}] Loading DatabaseOptions...");
        DatabaseOptions configuration = ConfigurationManager.Instance.Get<DatabaseOptions>();

        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Debug($"[DB.{nameof(AutoXDbContextFactory)}:{nameof(CreateDbContext)}] DatabaseType = {configuration.DatabaseType}, ConnectionString = {configuration.ConnectionString}");

        // Kiểm tra kết nối đến database
        if (!configuration.DatabaseType.Equals("SQLite", System.StringComparison.OrdinalIgnoreCase) &&
            !CAN_CONNECT_TO_DATABASE(configuration.ConnectionString))
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[DB.{nameof(AutoXDbContextFactory)}:{nameof(CreateDbContext)}] Cannot connect to the database at {configuration.ConnectionString}");
            throw new System.InvalidOperationException($"Cannot connect to the database at {configuration.ConnectionString}");
        }

        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Debug($"[DB.{nameof(AutoXDbContextFactory)}:{nameof(CreateDbContext)}] Database connectivity check passed.");

        DbContextOptionsBuilder<AutoXDbContext> optionsBuilder = new();

        try
        {
            if (configuration.DatabaseType.Equals("PostgreSQL", System.StringComparison.OrdinalIgnoreCase))
            {
                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                    .Debug($"[DB.{nameof(AutoXDbContextFactory)}:{nameof(CreateDbContext)}] Configuring DbContext for PostgreSQL.");

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
                                        .Info($"[DB.{nameof(AutoXDbContextFactory)}:{nameof(CreateDbContext)}] DbContext configured for PostgreSQL.");
            }
            else if (configuration.DatabaseType.Equals("SQLite", System.StringComparison.OrdinalIgnoreCase))
            {
                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                        .Debug($"[DB.{nameof(AutoXDbContextFactory)}:{nameof(CreateDbContext)}] Configuring DbContext for SQLite.");

                optionsBuilder.UseSqlite(configuration.ConnectionString, sqliteOptions =>
                    {
                        sqliteOptions.CommandTimeout(60);
                        sqliteOptions.MigrationsHistoryTable("__MigrationsHistory");
                    })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .EnableServiceProviderCaching();

                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                        .Info($"[DB.{nameof(AutoXDbContextFactory)}:{nameof(CreateDbContext)}] DbContext configured for SQLite.");
            }
            else
            {
                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                        .Warn($"[DB.{nameof(AutoXDbContextFactory)}:{nameof(CreateDbContext)}] Unsupported database type: {configuration.DatabaseType}");

                throw new System.InvalidOperationException($"Unsupported database type: {configuration.DatabaseType}");
            }
        }
        catch (System.Exception ex)
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[DB.{nameof(AutoXDbContextFactory)}:{nameof(CreateDbContext)}] Error configuring DbContext for {configuration.DatabaseType}.", ex);
            throw;
        }

        AutoXDbContext dbContext = new(optionsBuilder.Options);

        InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                .Info($"[DB.{nameof(AutoXDbContextFactory)}:{nameof(CreateDbContext)}] AutoDbContext successfully created.");

        return dbContext;
    }

    #region Private Methods

    private static System.Boolean CAN_CONNECT_TO_DATABASE(System.String connectionString)
    {
        try
        {
            Npgsql.NpgsqlConnectionStringBuilder builder = new(connectionString);

            System.String host = builder.Host;
            System.Int32 port = builder.Port;

            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Info($"[DB.{nameof(AutoXDbContextFactory)}:Internal] Pinging database server {host}:{port}...");

            using System.Net.NetworkInformation.Ping ping = new();
            System.Net.NetworkInformation.PingReply reply = ping.Send(host, 3000); // Timeout 1 giây

            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                        .Info($"[DB.{nameof(AutoXDbContextFactory)}:Internal] Ping to {host} successful.");
                return true;
            }

            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[DB.{nameof(AutoXDbContextFactory)}:Internal] Ping to {host} failed: {reply.Status}");
            return false;
        }
        catch (System.Exception ex)
        {
            InstanceManager.Instance.GetExistingInstance<ILogger>()?
                                    .Error($"[DB.{nameof(AutoXDbContextFactory)}:Internal] Error pinging database server.", ex);
            return false;
        }
    }

    #endregion Private Methods
}