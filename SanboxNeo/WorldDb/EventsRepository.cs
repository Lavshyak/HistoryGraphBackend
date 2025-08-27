using Neo4j.Driver;
using Neo4j.Driver.Preview.Mapping;

namespace SanboxNeo.WorldDb;

public class EventsRepository
{
    private readonly IAsyncSession _session;

    public EventsRepository(IAsyncSession session)
    {
        _session = session;
    }

    public async Task Init()
    {
        // Уникальный индекс по UUID (id)
        await _session.RunAsync("""
                                CREATE CONSTRAINT history_event_id_unique IF NOT EXISTS
                                FOR (e:HISTORY_EVENT)
                                REQUIRE e.id IS UNIQUE;
                                """);

        // Индекс по UUID (id)
        await _session.RunAsync($"CREATE INDEX rel_range_index_name FOR ()-[r:{RelationPureContinue.Label}]-() ON (r.id)");
        await _session.RunAsync($"CREATE INDEX rel_range_index_name FOR ()-[r:{RelationPureInfluenced.Label}]-() ON (r.id)");
        await _session.RunAsync($"CREATE INDEX rel_range_index_name FOR ()-[r:{RelationPureReferences.Label}]-() ON (r.id)");
        await _session.RunAsync($"CREATE INDEX rel_range_index_name FOR ()-[r:{RelationPureRelates.Label}]-() ON (r.id)");
        await _session.RunAsync($"CREATE INDEX rel_range_index_name FOR ()-[r:{RelationPureRelatesTheme.Label}]-() ON (r.id)");
    }

    public async Task<bool> AddEvents(HistoryEvent[] historyEvents)
    {
        foreach (var historyEvent in historyEvents)
        {
            if (historyEvent.Id == Guid.Empty)
            {
                historyEvent.Id = Guid.NewGuid();
            }
        }

        var events = historyEvents.Select(e => new
        {
            IdStr = e.Id.ToString(),
            TimeFrom = e.TimeFrom,
            TimeTo = e.TimeTo,
            Keywords = e.Keywords,
            Title = e.Title,
            Description = e.Description
        }).ToArray();

        var result = await _session.RunAsync($@"
                UNWIND $Events AS event
                MERGE (e:HISTORY_EVENT {{id: event.IdStr}})
                ON CREATE SET
                    e.time_from = datetime(event.TimeFrom),
                    e.time_to = datetime(event.TimeTo),
                    e.keywords = event.Keywords,
                    e.title = event.Title,
                    e.description = event.Description
                RETURN count(e) AS createdCount
            ",
            new
            {
                Events = events,
            }
        );

        var peek = await result.PeekAsync(); // работает нормально
        var createdCount = (await result.SingleAsync()); // 

        return true;
    }

    public async Task<bool> AddEvent(HistoryEvent historyEvent)
    {
        Guid id;
        if (historyEvent.Id == Guid.Empty)
        {
            id = Guid.NewGuid();
        }
        else
        {
            id = historyEvent.Id;
        }

        var result = await _session.RunAsync($@"""
                MERGE (e:HISTORY_EVENT {{e.id: $IdStr}})
                ON CREATE SET
                    e.time_from = datetime($TimeFrom),
                    e.time_to = datetime($TimeTo),
                    e.keywords = $Keywords,
                    e.title = $Title,
                    e.description = $Description
            """,
            new
            {
                Id = id,
                TimeFrom = historyEvent.TimeFrom,
                TimeTo = historyEvent.TimeTo,
                Keywords = historyEvent.Keywords,
                Title = historyEvent.Title,
                Description = historyEvent.Description,
            });

        historyEvent.Id = id;
        return true;
    }

    public class RelationsToAdd
    {
        public List<RelationPureContinue> Continues { get; set; } = [];
        public List<RelationPureInfluenced> Influenceds { get; set; } = [];
        public List<RelationPureReferences> Referencess { get; set; } = [];
        public List<RelationPureRelates> Relatess { get; set; } = [];
        public List<RelationPureRelatesTheme> ThemeRelatess { get; set; } = [];
    }

    public async Task<bool> AddRelations(RelationsToAdd relationsToAdd)
    {
        async Task<IResultCursor?> HandleRelations(
            IReadOnlyList<IRelationWithIds> relations,
            string relationLabel,
            string fromLabel,
            string toLabel)
        {
            if (relations.Count == 0) return null;

            // Проставляем Id, если их нет
            foreach (var r in relations)
            {
                if (r.Id == Guid.Empty)
                    r.Id = Guid.NewGuid();
            }

            // Подготавливаем данные для UNWIND
            var records = relations.Select(r => new
            {
                Id = r.Id.ToString(),
                FromId = r.FromId.ToString(),
                ToId = r.ToId.ToString()
            }).ToArray();

            var cypher = $@"
                UNWIND $Rels AS rel
                MATCH (from:{fromLabel} {{id: rel.FromId}})
                MATCH (to:{toLabel} {{id: rel.ToId}})
                MERGE (from)-[r:{relationLabel} {{id: rel.Id}}]->(to)
            ";

            var result = await _session.RunAsync(cypher, new { Rels = records });
            var resp = await result.ToListAsync();
            return result;
        }

        var r1 = await HandleRelations(
            relationsToAdd.Continues,
            RelationPureContinue.Label,
            RelationPureContinue.FromLabel,
            RelationPureContinue.ToLabel);

        var r2 = await HandleRelations(
            relationsToAdd.Influenceds,
            RelationPureInfluenced.Label,
            RelationPureInfluenced.FromLabel,
            RelationPureInfluenced.ToLabel);

        var r3 = await HandleRelations(
            relationsToAdd.Referencess,
            RelationPureReferences.Label,
            RelationPureReferences.FromLabel,
            RelationPureReferences.ToLabel);

        var r4 = await HandleRelations(
            relationsToAdd.Relatess,
            RelationPureRelates.Label,
            RelationPureRelates.FromLabel,
            RelationPureRelates.ToLabel);

        var r5 = await HandleRelations(
            relationsToAdd.ThemeRelatess,
            RelationPureRelatesTheme.Label,
            RelationPureRelatesTheme.FromLabel,
            RelationPureRelatesTheme.ToLabel);

        return true;
    }
}