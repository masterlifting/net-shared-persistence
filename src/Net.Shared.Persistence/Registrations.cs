using Microsoft.Extensions.DependencyInjection;

using Net.Shared.Persistence.Abstractions.Contexts;
using Net.Shared.Persistence.Abstractions.Repositories.NoSql;
using Net.Shared.Persistence.Abstractions.Repositories.Sql;
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
        switch (lifetime)
        {
            case ServiceLifetime.Scoped:
                services.AddScoped<T>();
                services.AddScoped<PostgreSqlContext, T>();
                services.AddScoped<IPersistenceSqlContext, T>();
                services.AddScoped<IPersistenceSqlReaderRepository, PostgreSqlReaderRepository>();
                services.AddScoped<IPersistenceSqlWriterRepository, PostgreSqlWriterRepository>();
                services.AddScoped<IPersistenceSqlProcessRepository, PostgreSqlProcessRepository>();
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton<T>();
                services.AddSingleton<PostgreSqlContext, T>();
                services.AddSingleton<IPersistenceSqlContext, T>();
                services.AddSingleton<IPersistenceSqlReaderRepository, PostgreSqlReaderRepository>();
                services.AddSingleton<IPersistenceSqlWriterRepository, PostgreSqlWriterRepository>();
                services.AddSingleton<IPersistenceSqlProcessRepository, PostgreSqlProcessRepository>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<T>();
                services.AddTransient<PostgreSqlContext, T>();
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
        switch (lifetime)
        {
            case ServiceLifetime.Scoped:
                services.AddScoped<T>();
                services.AddScoped<MongoDbContext, T>();
                services.AddScoped<IPersistenceNoSqlContext, T>();
                services.AddScoped<IPersistenceNoSqlReaderRepository, MongoDbReaderRepository>();
                services.AddScoped<IPersistenceNoSqlWriterRepository, MongoDbWriterRepository>();
                services.AddScoped<IPersistenceNoSqlProcessRepository, MongoDbProcessRepository>();
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton<T>();
                services.AddSingleton<MongoDbContext, T>();
                services.AddSingleton<IPersistenceNoSqlContext, T>();
                services.AddSingleton<IPersistenceNoSqlReaderRepository, MongoDbReaderRepository>();
                services.AddSingleton<IPersistenceNoSqlWriterRepository, MongoDbWriterRepository>();
                services.AddSingleton<IPersistenceNoSqlProcessRepository, MongoDbProcessRepository>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<T>();
                services.AddTransient<MongoDbContext, T>();
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
        switch (lifetime)
        {
            case ServiceLifetime.Scoped:
                services.AddScoped<T>();
                services.AddScoped<AzureTableContext, T>();
                services.AddScoped<IPersistenceNoSqlContext, T>();
                services.AddScoped<IPersistenceNoSqlReaderRepository, AzureTableReaderRepository>();
                services.AddScoped<IPersistenceNoSqlWriterRepository, AzureTableWriterRepository>();
                services.AddScoped<IPersistenceNoSqlProcessRepository, AzureTableProcessRepository>();
                break;
            case ServiceLifetime.Singleton:
                services.AddSingleton<T>();
                services.AddSingleton<AzureTableContext, T>();
                services.AddSingleton<IPersistenceNoSqlContext, T>();
                services.AddSingleton<IPersistenceNoSqlReaderRepository, AzureTableReaderRepository>();
                services.AddSingleton<IPersistenceNoSqlWriterRepository, AzureTableWriterRepository>();
                services.AddSingleton<IPersistenceNoSqlProcessRepository, AzureTableProcessRepository>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<T>();
                services.AddTransient<AzureTableContext, T>();
                services.AddTransient<IPersistenceNoSqlContext, T>();
                services.AddTransient<IPersistenceNoSqlReaderRepository, AzureTableReaderRepository>();
                services.AddTransient<IPersistenceNoSqlWriterRepository, AzureTableWriterRepository>();
                services.AddTransient<IPersistenceNoSqlProcessRepository, AzureTableProcessRepository>();
                break;
        }
    }
}
