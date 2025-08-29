using DbThings;
using Microsoft.Extensions.DependencyInjection;
using SeedNeo4jDb.WorldDb;

var services = new ServiceCollection();

services.AddDbThings(
    historyConfig: new Neo4jConnectionConfig("neo4j://localhost:7687", "neo4j", "w4gfe57GDF325hfw"),
    migrationsConfig: new Neo4jConnectionConfig("neo4j://localhost:7688", "neo4j", "w4gfe57GDF325hfw")
);

services.AddScoped<AddFromJson>();

var serviceProvider = services.BuildServiceProvider();

await using (var scope = serviceProvider.CreateAsyncScope())
{
    var ms = scope.ServiceProvider.GetRequiredService<MigrationsService>();
    await ms.Migrate();
    var afj = scope.ServiceProvider.GetRequiredService<AddFromJson>();
    await afj.ReadAndAdd();
}

Console.WriteLine("end.");