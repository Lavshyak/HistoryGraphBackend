using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
using Shared;

namespace DbThings;

public record Neo4jConnectionConfig(string Uri, string Username, string Password);

public static class ServiceCollectionExtensions
{
    public static void AddDbThings(this IServiceCollection services, Neo4jConnectionConfig historyConfig,
        Neo4jConnectionConfig migrationsConfig)
    {
        // history db
        {
            var config = historyConfig;
            var driver = GraphDatabase.Driver(config.Uri, AuthTokens.Basic(config.Username, config.Password),
                b => b.WithConnectionTimeout(TimeSpan.FromSeconds(60)));
            services.AddKeyedSingleton<IDriver>(ServiceKeys.Neo4jHistoryDriver, driver);
            services.AddKeyedScoped<IAsyncSession>(ServiceKeys.Neo4jHistorySession, (_, _) => driver.AsyncSession());

            // defaults
            services.AddSingleton<IDriver>(driver);
            services.AddScoped<IAsyncSession>(_ => driver.AsyncSession());
        }

        // migrations db
        {
            var config = migrationsConfig;
            var driver = GraphDatabase.Driver(config.Uri, AuthTokens.Basic(config.Username, config.Password),
                b => b.WithConnectionTimeout(TimeSpan.FromSeconds(60)));
            services.AddKeyedSingleton<IDriver>(ServiceKeys.Neo4jMigrationsDriver, driver);
            services.AddKeyedScoped<IAsyncSession>(ServiceKeys.Neo4jMigrationsSession, (_, _) => driver.AsyncSession());
        }

        services.AddScoped<DataBase>();
        services.AddScoped<MigrationsService>();
        services.AddScoped<EventsRepository>();
    }
}