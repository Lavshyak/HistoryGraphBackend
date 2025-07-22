using ArangoDBNetStandard;
using ArangoDBNetStandard.Transport.Http;

Console.WriteLine("=== Historical Graph Creation Tool ===");
Console.WriteLine("Connecting to ArangoDB...");

// Create HTTP transport for ArangoDB (connect to _system first)
var systemTransport = HttpApiTransport.UsingBasicAuth(
    new Uri("http://localhost:8529/"),
    "_system",
    null,
    null);

// Create ArangoDB client for system database
var systemDb = new ArangoDBClient(systemTransport);

// Create our database
try
{
    await systemDb.Database.PostDatabaseAsync(new ArangoDBNetStandard.DatabaseApi.Models.PostDatabaseBody
    {
        Name = "history_graph"
    });
    Console.WriteLine("✓ Database 'history_graph' created");
}
catch (Exception ex)
{
    Console.WriteLine($"Database 'history_graph' already exists or error: {ExceptionSerializer.SerializeException(ex)}\n");
}

// Now connect to our database
var transport = HttpApiTransport.UsingBasicAuth(
    new Uri("http://localhost:8529/"),
    "history_graph",
    null,
    null);

// Create ArangoDB client for our database
var db = new ArangoDBClient(transport);

// Create graph
try
{
    await db.Graph.PostGraphAsync(new ArangoDBNetStandard.GraphApi.Models.PostGraphBody
    {
        Name = "historical_events",
        EdgeDefinitions = new List<ArangoDBNetStandard.GraphApi.Models.EdgeDefinition>
        {
            new()
            {
                Collection = "event_relations",
                From = new[] { "events" },
                To = new[] { "events" }
            }
        }
    });
    Console.WriteLine("✓ Graph 'historical_events' created");
}
catch (Exception ex)
{
    Console.WriteLine($"Graph 'historical_events' already exists or error: {ex.Message}");
}

try
{
    // Define three historical events
    var events = new[]
    {
        new
        {
            _key = "ww2_start",
            name = "Начало Второй мировой войны",
            date = "1939-09-01",
            description = "Германия вторгается в Польшу, начиная Вторую мировую войну",
            type = "war"
        },
        new
        {
            _key = "pearl_harbor",
            name = "Атака на Пёрл-Харбор",
            date = "1941-12-07",
            description = "Япония атакует военно-морскую базу США на Гавайях",
            type = "military_operation"
        },
        new
        {
            _key = "d_day",
            name = "Высадка в Нормандии",
            date = "1944-06-06",
            description = "Союзные войска начинают операцию Оверлорд, открывая второй фронт в Европе",
            type = "military_operation"
        }
    };

    // Insert events into the database
    int eventsCreated = 0;
    foreach (var evt in events)
    {
        try
        {
            await db.Document.PostDocumentAsync("events", evt);
            Console.WriteLine($"✓ Event '{evt.name}' inserted");
            eventsCreated++;
        }
        catch
        {
            Console.WriteLine($"  Event '{evt.name}' already exists");
        }
    }

    // Create relationships between events
    var relationships = new[]
    {
        new
        {
            _key = "rel1",
            _from = "events/ww2_start",
            _to = "events/pearl_harbor",
            type = "led_to",
            description = "Вторая мировая война привела к вовлечению США после атаки на Пёрл-Харбор"
        },
        new
        {
            _key = "rel2",
            _from = "events/pearl_harbor",
            _to = "events/d_day",
            type = "preceded",
            description = "После вступления США в войну союзники подготовили высадку в Нормандии"
        },
        new
        {
            _key = "rel3",
            _from = "events/ww2_start",
            _to = "events/d_day",
            type = "culminated_in",
            description = "Вторая мировая война завершилась высадкой союзников в Европе"
        }
    };

    // Insert relationships
    int relationsCreated = 0;
    foreach (var rel in relationships)
    {
        try
        {
            await db.Document.PostDocumentAsync("event_relations", rel);
            Console.WriteLine($"✓ Relationship created: {rel.type}");
            relationsCreated++;
        }
        catch
        {
            Console.WriteLine($"  Relationship already exists");
        }
    }

    // Query and display the created events
    Console.WriteLine("\n=== Historical Events in Database ===");
    var eventsQuery = "FOR e IN events SORT e.date RETURN e";
    var eventsCursor = await db.Cursor.PostCursorAsync<dynamic>(eventsQuery);
    
    int eventCount = 0;
    foreach (var evt in eventsCursor.Result)
    {
        Console.WriteLine($"{++eventCount}. {evt.name}");
        Console.WriteLine($"   Date: {evt.date}");
        Console.WriteLine($"   Description: {evt.description}");
        Console.WriteLine($"   Type: {evt.type}");
        Console.WriteLine();
    }

    // Query and display the relationships
    Console.WriteLine("=== Relationships in Graph ===");
    var relsQuery = @"
FOR e IN event_relations
LET fromEvent = DOCUMENT(e._from)
LET toEvent = DOCUMENT(e._to)
SORT fromEvent.date, toEvent.date
RETURN {
    from: fromEvent.name,
    to: toEvent.name,
    type: e.type,
    description: e.description
}";

    var relsCursor = await db.Cursor.PostCursorAsync<dynamic>(relsQuery);
    
    int relCount = 0;
    foreach (var rel in relsCursor.Result)
    {
        Console.WriteLine($"{++relCount}. {rel.from} → {rel.to}");
        Console.WriteLine($"   Type: {rel.type}");
        Console.WriteLine($"   Description: {rel.description}");
        Console.WriteLine();
    }

    // Show graph structure
    Console.WriteLine("=== Graph Structure ===");
    var graphQuery = @"
FOR v, e, p IN 1..2 OUTBOUND 'events/ww2_start' 
GRAPH 'historical_events'
RETURN {
    event_name: v.name,
    relation_type: e.type
}";

    var graphCursor = await db.Cursor.PostCursorAsync<dynamic>(graphQuery);
    
    Console.WriteLine("Starting from: Начало Второй мировой войны");
    foreach (var path in graphCursor.Result)
    {
        Console.WriteLine($"  → {path.event_name} (via: {path.relation_type})");
    }

    // Display graph summary
    Console.WriteLine($"\n=== Graph Summary ===");
    Console.WriteLine($"✓ Database: history_graph");
    Console.WriteLine($"✓ Graph: historical_events");
    Console.WriteLine($"✓ Events: {eventCount} total");
    Console.WriteLine($"✓ Relationships: {relCount} total");
    Console.WriteLine("\nHistorical graph created successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine("Make sure ArangoDB is running on localhost:8529");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

