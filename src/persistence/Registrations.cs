using Azure.Data.Tables;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Persistence.Abstractions.Interfaces.Contexts;
using Net.Shared.Persistence.Abstractions.Interfaces.Entities;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.NoSql;
using Net.Shared.Persistence.Abstractions.Interfaces.Repositories.Sql;
using Net.Shared.Persistence.Abstractions.Models.Settings.Connections;
using Net.Shared.Persistence.Contexts;
using Net.Shared.Persistence.Repositories.AzureTable;
using Net.Shared.Persistence.Repositories.MongoDb;
using Net.Shared.Persistence.Repositories.PostgreSql;

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
    public static void AddPostgreSql<T>(this IServiceCollection services, ServiceLifetime lifetime) where T : PostgreSqlContext
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
                services.AddScoped<PostgreSqlContext, T>();
                services.AddScoped<IPersistenceContext<IPersistentSql>, T>();
                services.AddScoped<IPersistenceReaderRepository<IPersistentSql>, PostgreSqlReaderRepository>();
                services.AddScoped<IPersistenceWriterRepository<IPersistentSql>, PostgreSqlWriterRepository>();
                services.AddScoped<IPersistenceProcessRepository<IPersistentSql>, PostgreSqlProcessRepository>();
                services.AddScoped<IPersistenceSqlContext, T>();
                services.AddScoped<IPersistenceSqlReaderRepository, PostgreSqlReaderRepository>();
                services.AddScoped<IPersistenceSqlWriterRepository, PostgreSqlWriterRepository>();
                services.AddScoped<IPersistenceSqlProcessRepository, PostgreSqlProcessRepository>();
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton<T>();
                services.AddSingleton<PostgreSqlContext, T>();
                services.AddSingleton<IPersistenceContext<IPersistentSql>, T>();
                services.AddSingleton<IPersistenceReaderRepository<IPersistentSql>, PostgreSqlReaderRepository>();
                services.AddSingleton<IPersistenceWriterRepository<IPersistentSql>, PostgreSqlWriterRepository>();
                services.AddSingleton<IPersistenceProcessRepository<IPersistentSql>, PostgreSqlProcessRepository>();
                services.AddSingleton<IPersistenceSqlContext, T>();
                services.AddSingleton<IPersistenceSqlReaderRepository, PostgreSqlReaderRepository>();
                services.AddSingleton<IPersistenceSqlWriterRepository, PostgreSqlWriterRepository>();
                services.AddSingleton<IPersistenceSqlProcessRepository, PostgreSqlProcessRepository>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<T>();
                services.AddTransient<PostgreSqlContext, T>();
                services.AddTransient<IPersistenceContext<IPersistentSql>, T>();
                services.AddTransient<IPersistenceReaderRepository<IPersistentSql>, PostgreSqlReaderRepository>();
                services.AddTransient<IPersistenceWriterRepository<IPersistentSql>, PostgreSqlWriterRepository>();
                services.AddTransient<IPersistenceProcessRepository<IPersistentSql>, PostgreSqlProcessRepository>();
                services.AddTransient<IPersistenceSqlContext, T>();
                services.AddTransient<IPersistenceSqlReaderRepository, PostgreSqlReaderRepository>();
                services.AddTransient<IPersistenceSqlWriterRepository, PostgreSqlWriterRepository>();
                services.AddTransient<IPersistenceSqlProcessRepository, PostgreSqlProcessRepository>();
                break;
        }
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
    public static void AddMongoDb<T>(this IServiceCollection services, ServiceLifetime lifetime) where T : MongoDbContext
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
                services.AddScoped<MongoDbContext, T>();
                services.AddScoped<IPersistenceContext<IPersistentNoSql>, T>();
                services.AddScoped<IPersistenceReaderRepository<IPersistentNoSql>, MongoDbReaderRepository>();
                services.AddScoped<IPersistenceWriterRepository<IPersistentNoSql>, MongoDbWriterRepository>();
                services.AddScoped<IPersistenceProcessRepository<IPersistentNoSql>, MongoDbProcessRepository>();
                services.AddScoped<IPersistenceNoSqlContext, T>();
                services.AddScoped<IPersistenceNoSqlReaderRepository, MongoDbReaderRepository>();
                services.AddScoped<IPersistenceNoSqlWriterRepository, MongoDbWriterRepository>();
                services.AddScoped<IPersistenceNoSqlProcessRepository, MongoDbProcessRepository>();
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton<T>();
                services.AddSingleton<MongoDbContext, T>();
                services.AddSingleton<IPersistenceContext<IPersistentNoSql>, T>();
                services.AddSingleton<IPersistenceReaderRepository<IPersistentNoSql>, MongoDbReaderRepository>();
                services.AddSingleton<IPersistenceWriterRepository<IPersistentNoSql>, MongoDbWriterRepository>();
                services.AddSingleton<IPersistenceProcessRepository<IPersistentNoSql>, MongoDbProcessRepository>();
                services.AddSingleton<IPersistenceNoSqlContext, T>();
                services.AddSingleton<IPersistenceNoSqlReaderRepository, MongoDbReaderRepository>();
                services.AddSingleton<IPersistenceNoSqlWriterRepository, MongoDbWriterRepository>();
                services.AddSingleton<IPersistenceNoSqlProcessRepository, MongoDbProcessRepository>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<T>();
                services.AddTransient<MongoDbContext, T>();
                services.AddTransient<IPersistenceContext<IPersistentNoSql>, T>();
                services.AddTransient<IPersistenceReaderRepository<IPersistentNoSql>, MongoDbReaderRepository>();
                services.AddTransient<IPersistenceWriterRepository<IPersistentNoSql>, MongoDbWriterRepository>();
                services.AddTransient<IPersistenceProcessRepository<IPersistentNoSql>, MongoDbProcessRepository>();
                services.AddTransient<IPersistenceNoSqlContext, T>();
                services.AddTransient<IPersistenceNoSqlReaderRepository, MongoDbReaderRepository>();
                services.AddTransient<IPersistenceNoSqlWriterRepository, MongoDbWriterRepository>();
                services.AddTransient<IPersistenceNoSqlProcessRepository, MongoDbProcessRepository>();
                break;
        }
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
    public static void AddAzureTable<T>(this IServiceCollection services, ServiceLifetime lifetime) where T : AzureTableContext
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
                services.AddScoped<AzureTableContext, T>();
                services.AddScoped<IPersistenceContext<ITableEntity>, T>();
                services.AddScoped<IPersistenceReaderRepository<ITableEntity>, AzureTableReaderRepository>();
                services.AddScoped<IPersistenceWriterRepository<ITableEntity>, AzureTableWriterRepository>();
                services.AddScoped<IPersistenceProcessRepository<ITableEntity>, AzureTableProcessRepository>();
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton<T>();
                services.AddSingleton<AzureTableContext, T>();
                services.AddSingleton<IPersistenceContext<ITableEntity>, T>();
                services.AddSingleton<IPersistenceReaderRepository<ITableEntity>, AzureTableReaderRepository>();
                services.AddSingleton<IPersistenceWriterRepository<ITableEntity>, AzureTableWriterRepository>();
                services.AddSingleton<IPersistenceProcessRepository<ITableEntity>, AzureTableProcessRepository>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<T>();
                services.AddTransient<AzureTableContext, T>();
                services.AddTransient<IPersistenceContext<ITableEntity>, T>();
                services.AddTransient<IPersistenceReaderRepository<ITableEntity>, AzureTableReaderRepository>();
                services.AddTransient<IPersistenceWriterRepository<ITableEntity>, AzureTableWriterRepository>();
                services.AddTransient<IPersistenceProcessRepository<ITableEntity>, AzureTableProcessRepository>();
                break;
        }
    }
}
