# SanboxNeo - Historical Graph Creation Tool (Neo4j)

This project creates a historical graph database in Neo4j using the same TSV data files as the ArangoDB version.

## Prerequisites

1. **Neo4j Database** running on localhost:7687
   - Download from: https://neo4j.com/download/
   - Default credentials: neo4j/yourpassword (change in Program.cs if needed)

2. **.NET 8.0** or later
   - Download from: https://dotnet.microsoft.com/download

3. **TSV Data Files** in the parent directory:
   - `../events.tsv` - Historical events data
   - `../relationships.tsv` - Relationships between events

## Setup

1. **Install Neo4j**:
   ```bash
   # Using Docker (recommended)
   docker run \
     --publish=7474:7474 --publish=7687:7687 \
     --volume=$HOME/neo4j/data:/data \
     --env NEO4J_AUTH=neo4j/yourpassword \
     neo4j:latest
   ```

2. **Update credentials** (if needed):
   Edit `Program.cs` and change the connection string:
   ```csharp
   var driver = GraphDatabase.Driver("bolt://localhost:7687", 
       AuthTokens.Basic("neo4j", "yourpassword"));
   ```

3. **Build the project**:
   ```bash
   cd SanboxNeo
   dotnet restore
   dotnet build
   ```

## Usage

Run the application:
```bash
dotnet run
```

The tool will:
1. Connect to Neo4j
2. Create necessary constraints
3. Load events from `../events.tsv`
4. Load relationships from `../relationships.tsv`
5. Display the created graph structure

## Data Structure

### Events (Nodes)
- **Label**: `Event`
- **Properties**:
  - `id`: Unique identifier
  - `name`: Event name
  - `date`: Event date
  - `description`: Event description
  - `type`: Event type/category

### Relationships (Edges)
- **Type**: `RELATES_TO`
- **Properties**:
  - `id`: Unique identifier
  - `type`: Relationship type
  - `description`: Relationship description

## Example Output

```
=== Historical Graph Creation Tool (Neo4j) ===
Connecting to Neo4j...
Creating constraints...
✓ Event ID constraint created

Loading events from events.tsv...
✓ Event 'World War II Begins' inserted
✓ Event 'Pearl Harbor Attack' inserted
...

=== Historical Events in Database ===
1. World War II Begins
   Date: 1939-09-01
   Description: Germany invades Poland, starting WWII
   Type: War

=== Relationships in Graph ===
1. World War II Begins → Pearl Harbor Attack
   Type: leads_to
   Description: WWII leads to US entry after Pearl Harbor
...

=== Graph Summary ===
✓ Database: Neo4j
✓ Events: 15 total
✓ Relationships: 20 total
```

## Troubleshooting

### Connection Issues
- Ensure Neo4j is running: `docker ps` or check Neo4j Desktop
- Verify port 7687 is accessible: `telnet localhost 7687`
- Check credentials in Program.cs match your Neo4j setup

### File Not Found
- Ensure `events.tsv` and `relationships.tsv` exist in the parent directory
- Check file permissions

### Build Issues
- Run `dotnet restore` to install dependencies
- Ensure .NET 8.0 SDK is installed: `dotnet --version`

## Neo4j Browser

After running the tool, you can explore the graph using Neo4j Browser:
- Open: http://localhost:7474
- Login with your credentials
- Run Cypher queries like:
  ```cypher
  MATCH (e:Event) RETURN e LIMIT 10
  MATCH (a)-[r:RELATES_TO]->(b) RETURN a.name, type(r), b.name
  ```

## Comparison with ArangoDB Version

| Feature | ArangoDB | Neo4j |
|---------|----------|--------|
| **Database Type** | Multi-model (Document/Graph) | Graph Database |
| **Query Language** | AQL | Cypher |
| **Connection** | HTTP REST API | Bolt Protocol |
| **Schema** | Collections + Graph | Nodes + Relationships |
| **Performance** | Good for complex queries | Excellent for graph traversal |
