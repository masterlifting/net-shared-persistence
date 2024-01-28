using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Persistence.Abstractions.Models.Settings.Connections;
using Net.Shared.Persistence.Contexts;

namespace Net.Shared.Persistence;

public static class Registrations
{
    /// <summary>
    /// Add PostgreSql Context.
    /// </summary>
    /// <param name="services">
    /// <see cref="IServiceCollection"/> to add the context to.
    /// </param>
    /// <param name="lifetime">
    /// <see cref="ServiceLifetime"/> of the context.
    /// </param>
    /// <typeparam name="T">
    /// <see cref="PostgreSqlContext"/> type of the context.
    /// </typeparam>
    public static IServiceCollection AddPostgreSql<T>(this IServiceCollection services, ServiceLifetime lifetime) where T : PostgreSqlContext
    {
        services
            .AddOptions<PostgreSqlConnectionSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration
                    .GetSection(PostgreSqlConnectionSettings.SectionName)
                    .Bind(settings);
            });

        switch (lifetime)
        {
            case ServiceLifetime.Scoped:
                services.AddScoped<T>();
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton<T>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<T>();
                break;
        }

        return services;
    }

    /// <summary>
    /// Add MongoDb Context.
    /// </summary>
    /// <param name="services">
    /// <see cref="IServiceCollection"/> to add the context to.
    /// </param>
    /// <param name="lifetime">
    /// <see cref="ServiceLifetime"/> of the context.
    /// </param>
    /// <typeparam name="T">
    /// <see cref="MongoDbContext"/> type of the context.
    /// </typeparam>
    public static IServiceCollection AddMongoDb<T>(this IServiceCollection services, ServiceLifetime lifetime) where T : MongoDbContext
    {
        services
            .AddOptions<MongoDbConnectionSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration
                    .GetSection(MongoDbConnectionSettings.SectionName)
                    .Bind(settings);
            });

        switch (lifetime)
        {
            case ServiceLifetime.Scoped:
                services.AddScoped<T>();
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton<T>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<T>();
                break;
        }

        return services;
    }

    /// <summary>
    /// Add AzureTable Context.
    /// </summary>
    /// <param name="services">
    /// <see cref="IServiceCollection"/> to add the context to.
    /// </param>
    /// <param name="lifetime">
    /// <see cref="ServiceLifetime"/> of the context.
    /// </param>
    /// <typeparam name="T">
    /// <see cref="AzureTableContext"/> type of the context.
    /// </typeparam>
    public static IServiceCollection AddAzureTable<T>(this IServiceCollection services, ServiceLifetime lifetime) where T : AzureTableContext
    {
        services
            .AddOptions<AzureTableConnectionSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration
                    .GetSection(AzureTableConnectionSettings.SectionName)
                    .Bind(settings);
            });

        switch (lifetime)
        {
            case ServiceLifetime.Scoped:
                services.AddScoped<T>();
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton<T>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<T>();
                break;
        }

        return services;
    }
}
