using Neo4j.Driver;
using System.Globalization;
using SanboxNeo.WorldDb;

var driver = GraphDatabase.Driver("neo4j://localhost:7687", 
    AuthTokens.Basic("neo4j", "w4gfe57GDF325hfw"));

await using (var session = driver.AsyncSession())
{
    var repository = new EventsRepository(session);
    var addFromJson = new AddFromJson(repository);
    await addFromJson.ReadAndAdd();
}