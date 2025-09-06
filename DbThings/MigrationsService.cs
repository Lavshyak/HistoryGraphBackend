using DbThings.PureEntities;
using Microsoft.Extensions.DependencyInjection;
using Neo4j.Driver;
using Shared;

namespace DbThings;

public class MigrationsService
{
    private readonly DataBase _dataBase;
    private readonly IAsyncSession _sessionMigrations;

    public MigrationsService(
        DataBase dataBase,
        [FromKeyedServices(ServiceKeys.Neo4jMigrationsSession)]
        IAsyncSession sessionMigrations
    )
    {
        _dataBase = dataBase;
        _sessionMigrations = sessionMigrations;
    }

    public record Migration(string Id, string Description)
    {
        public const string Label = "MIGRATION";
    }

    public async Task<bool> GetIsMigrationApplied(string id)
    {
        var cursor = await _sessionMigrations.RunAsync($@"
            RETURN EXISTS {{
                    MATCH (m:{Migration.Label} {{id: $id}})
            }} AS exists
        ", new
        {
            id = id
        });

        var record = await cursor.SingleAsync();
        bool exists = record["exists"].As<bool>();

        return exists;
    }

    public async Task TryApplyMigrationToHistory(string id, string description, Func<IAsyncQueryRunner, Task> func)
    {
        var isApplied = await GetIsMigrationApplied(id);
        if (!isApplied)
        {
            try
            {
                await using var transaction1 = await _dataBase.BeginTransactionAsync();
                await BeginMigration(_sessionMigrations, new Migration(id, description));
                try
                {
                    await func(transaction1);
                    await transaction1.CommitAsync();
                    await FinishMigration(_sessionMigrations, id);
                }
                catch (Exception ex)
                {
                    await transaction1.RollbackAsync();
                    throw;
                }
            }
            catch
            {
                await EnsureMigrationDeleted(_sessionMigrations, id);
                throw;
            }
        }
    }

    public async Task TryApplyMigrationToMigrations(string id, string description, Func<IAsyncQueryRunner, Task> func)
    {
        var isApplied = await GetIsMigrationApplied(id);
        if (!isApplied)
        {
            try
            {
                await using var transaction1 = await _sessionMigrations.BeginTransactionAsync();
                await BeginMigration(_sessionMigrations, new Migration(id, description));
                try
                {
                    await func(transaction1);
                    await transaction1.CommitAsync();
                    await FinishMigration(_sessionMigrations, id);
                }
                catch (Exception ex)
                {
                    await transaction1.RollbackAsync();
                    throw;
                }
            }
            catch
            {
                await EnsureMigrationDeleted(_sessionMigrations, id);
                throw;
            }
        }
    }

    public async Task BeginMigration(IAsyncQueryRunner transaction, Migration migration)
    {
        var cursor = await transaction.RunAsync($@"
             MERGE (m:{Migration.Label} {{id: $id}})
             ON CREATE SET
                m.description = $description
                m.finished = false
             RETURN count(m) AS createdCount
        ", new
        {
            id = migration.Id,
            description = migration.Description,
        });

        var list = await cursor.ToListAsync();
    }

    public async Task FinishMigration(IAsyncQueryRunner transaction, string migrationId)
    {
        var cursor = await transaction.RunAsync($@"
             MATCH (m:{Migration.Label} {{id: $id}})
             SET m.finished = true
        ", new
        {
            id = migrationId,
        });

        var list = await cursor.ToListAsync();
    }

    public async Task EnsureMigrationDeleted(IAsyncQueryRunner transaction, string migrationId)
    {
        var cursor = await transaction.RunAsync($@"
             MATCH (m:{Migration.Label} {{id: $id}})
             DETACH DELETE m
        ", new
        {
            id = migrationId,
        });

        var list = await cursor.ToListAsync();
    }

    public async Task Migrate()
    {
        await TryApplyMigrationToMigrations("1", "Уникальный индекс миграций по id", async transaction =>
        {
            // Уникальный индекс миграций по id
            var cursor = await transaction.RunAsync($@"
                 CREATE CONSTRAINT {Migration.Label}_id_unique IF NOT EXISTS
                 FOR (m:{Migration.Label})
                 REQUIRE m.id IS UNIQUE;
            ");
            var list = await cursor.ToListAsync();
        });

        await TryApplyMigrationToHistory("2", "Индексы евента и связей по id", async transaction =>
        {
            // Уникальный индекс по UUID (id)
            var cursor = await transaction.RunAsync(@"
                CREATE CONSTRAINT history_event_id_unique IF NOT EXISTS
                FOR (e:HISTORY_EVENT)
                REQUIRE e.id IS UNIQUE;
            ");
            var list = await cursor.ToListAsync();

            // Индекс по UUID (id)
            var c1 = await transaction.RunAsync(
                $"CREATE INDEX rel_range_index_{PureRelationContinue.Label} FOR ()-[r:{PureRelationContinue.Label}]-() ON (r.id)");
            var l1 = await c1.ToListAsync();
            var c2 = await transaction.RunAsync(
                $"CREATE INDEX rel_range_index_{PureRelationPureInfluenced.Label} FOR ()-[r:{PureRelationPureInfluenced.Label}]-() ON (r.id)");
            var l2 = await c2.ToListAsync();
            var c3 = await transaction.RunAsync(
                $"CREATE INDEX rel_range_index_{PureRelationPureReferences.Label} FOR ()-[r:{PureRelationPureReferences.Label}]-() ON (r.id)");
            var l3 = await c2.ToListAsync();
            var c4 = await transaction.RunAsync(
                $"CREATE INDEX rel_range_index_{PureRelationPureRelates.Label} FOR ()-[r:{PureRelationPureRelates.Label}]-() ON (r.id)");
            var l4 = await c3.ToListAsync();
            var c5 = await transaction.RunAsync(
                $"CREATE INDEX rel_range_index_{PureRelationPureRelatesTheme.Label} FOR ()-[r:{PureRelationPureRelatesTheme.Label}]-() ON (r.id)");
            var l5 = await c4.ToListAsync();
        });
    }
}