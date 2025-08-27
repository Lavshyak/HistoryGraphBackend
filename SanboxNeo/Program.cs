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

return;

/*
Console.WriteLine("=== Historical Graph Creation Tool (Neo4j) ===");
Console.WriteLine("Connecting to Neo4j...");

// Create Neo4j driver
var driver = GraphDatabase.Driver("bolt://localhost:7687", 
    AuthTokens.Basic("neo4j", "yourpassword"));

await using (var session = driver.AsyncSession())
{
    try
    {
        // Create constraints for uniqueness
        Console.WriteLine("Creating constraints...");
        await session.RunAsync("CREATE CONSTRAINT IF NOT EXISTS FOR (e:Event) REQUIRE e.id IS UNIQUE");
        Console.WriteLine("✓ Event ID constraint created");

        // Load events from TSV file
        Console.WriteLine("\nLoading events from events.tsv...");
        var events = new List<dynamic>();
        var eventLines = await File.ReadAllLinesAsync("./events.tsv");
        
        // Skip header and process each line
        for (int i = 1; i < eventLines.Length; i++)
        {
            var parts = eventLines[i].Split('\t');
            if (parts.Length >= 5)
            {
                events.Add(new
                {
                    id = parts[0],
                    name = parts[1],
                    date = parts[2],
                    description = parts[3],
                    type = parts[4]
                });
            }
        }

        // Insert events into Neo4j
        int eventsCreated = 0;
        foreach (var evt in events)
        {
            var result = await session.RunAsync(
                @"MERGE (e:Event {id: $id})
                  ON CREATE SET 
                    e.name = $name,
                    e.date = $date,
                    e.description = $description,
                    e.type = $type
                  RETURN e, e.id IS NOT NULL as created",
                new { evt.id, evt.name, evt.date, evt.description, evt.type });

            var record = await result.SingleAsync();
            if (record["created"].As<bool>())
            {
                Console.WriteLine($"✓ Event '{evt.name}' inserted");
                eventsCreated++;
            }
            else
            {
                Console.WriteLine($"  Event '{evt.name}' already exists - skipping");
            }
        }

        // Load relationships from TSV file
        Console.WriteLine("\nLoading relationships from relationships.tsv...");
        var relationships = new List<dynamic>();
        var relLines = await File.ReadAllLinesAsync("./relationships.tsv");
        
        // Skip header and process each line
        for (int i = 1; i < relLines.Length; i++)
        {
            var parts = relLines[i].Split('\t');
            if (parts.Length >= 5)
            {
                relationships.Add(new
                {
                    id = parts[0],
                    fromId = parts[1],
                    toId = parts[2],
                    type = parts[3],
                    description = parts[4]
                });
            }
        }

        // Insert relationships
        int relationsCreated = 0;
        foreach (var rel in relationships)
        {
            var result = await session.RunAsync(
                @"MATCH (from:Event {id: $fromId}), (to:Event {id: $toId})
                  MERGE (from)-[r:RELATES_TO {
                    id: $id,
                    type: $type,
                    description: $description
                  }]->(to)
                  RETURN r, r.id IS NOT NULL as created",
                new { rel.id, rel.fromId, rel.toId, rel.type, rel.description });

            var record = await result.SingleAsync();
            if (record["created"].As<bool>())
            {
                Console.WriteLine($"✓ Relationship created: {rel.type}");
                relationsCreated++;
            }
            else
            {
                Console.WriteLine($"  Relationship already exists - skipping");
            }
        }

        // Query and display the created events
        Console.WriteLine("\n=== Historical Events in Database ===");
        var eventsQuery = await session.RunAsync(
            "MATCH (e:Event) RETURN e ORDER BY e.date");

        int eventCount = 0;
        await foreach (var record in eventsQuery)
        {
            var evt = record["e"].As<INode>();
            Console.WriteLine($"{++eventCount}. {evt["name"]}");
            Console.WriteLine($"   Date: {evt["date"]}");
            Console.WriteLine($"   Description: {evt["description"]}");
            Console.WriteLine($"   Type: {evt["type"]}");
            Console.WriteLine();
        }

        // Query and display the relationships
        Console.WriteLine("=== Relationships in Graph ===");
        var relsQuery = await session.RunAsync(
            @"MATCH (from:Event)-[r:RELATES_TO]->(to:Event)
              RETURN from.name as from, to.name as to, r.type as type, r.description as description
              ORDER BY from.date, to.date");

        int relCount = 0;
        await foreach (var record in relsQuery)
        {
            Console.WriteLine($"{++relCount}. {record["from"]} → {record["to"]}");
            Console.WriteLine($"   Type: {record["type"]}");
            Console.WriteLine($"   Description: {record["description"]}");
            Console.WriteLine();
        }

        // Show graph structure starting from WWII
        Console.WriteLine("=== Graph Structure ===");
        var graphQuery = await session.RunAsync(
            @"MATCH path = (start:Event {id: 'ww2_start'})-[*1..2]->(connected:Event)
              RETURN start.name as start_event, 
                     [rel in relationships(path) | rel.type] as relation_types,
                     [node in nodes(path)[1..] | node.name] as connected_events");

        var graphResult = await graphQuery.SingleAsync();
        Console.WriteLine($"Starting from: {graphResult["start_event"]}");
        
        var connectedEvents = graphResult["connected_events"].As<List<string>>();
        var relationTypes = graphResult["relation_types"].As<List<string>>();
        
        for (int i = 0; i < connectedEvents.Count; i++)
        {
            var relationType = i < relationTypes.Count ? relationTypes[i] : "related";
            Console.WriteLine($"  → {connectedEvents[i]} (via: {relationType})");
        }

        // Display graph summary
        Console.WriteLine($"\n=== Graph Summary ===");
        Console.WriteLine($"✓ Database: Neo4j");
        Console.WriteLine($"✓ Events: {eventCount} total");
        Console.WriteLine($"✓ Relationships: {relCount} total");
        Console.WriteLine("\nHistorical graph created successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

await driver.DisposeAsync();

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
*/
