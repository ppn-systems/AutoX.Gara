// Copyright (c) 2026 PPN Corporation. All rights reserved.

using Nalix.Framework.Configuration.Binding;
using Nalix.Framework.Environment;

namespace AutoX.Gara.Infrastructure.Configuration;

/// <summary>
/// Database configuration settings.
/// </summary>
public sealed class DatabaseOptions : ConfigurationLoader
{
    /// <summary>
    /// Database type (PostgreSQL | SQLite).
    /// </summary>
    public System.String DatabaseType { get; init; } = "SQLite";

    /// <summary>
    /// Default database connection string.
    /// </summary>
    public System.String ConnectionString { get; init; } = $"Data Source={System.IO.Path.Combine(Directories.DatabaseDirectory, "AutoX.db")}";
}
