using Microsoft.Extensions.DependencyInjection;
using AutoX.Gara.Application.Abstractions.Persistence;
using AutoX.Gara.Infrastructure.Database;
using AutoX.Gara.Infrastructure.Persistence;

namespace AutoX.Gara.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Đăng ký tất cả Infrastructure components vào DI Container.
    /// Bao gồm Persistence layer, Database context và Middleware hẹ tầng.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Database Infrastructure
        services.AddSingleton<AutoXDbContextFactory>();
        services.AddScoped<IDataSessionFactory>(sp => new DataSessionFactory(sp.GetRequiredService<AutoXDbContextFactory>()));

        return services;
    }
}
