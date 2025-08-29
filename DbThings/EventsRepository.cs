using DbThings.PureEntities;
using Neo4j.Driver;

namespace DbThings;

public class EventsRepository
{
    private readonly IAsyncSession _session;

    public EventsRepository(IAsyncSession session)
    {
        _session = session;
    }
    
    public async Task<IAsyncTransaction> BeginTransactionAsync() => await _session.BeginTransactionAsync();

    public record EventsAndRelationships(PureHistoryEvent[] Events, PureRelationWithIdsAndLabel[] Relations);
    public async Task<EventsAndRelationships> GetAll()
    {
        var result = await _session.RunAsync(@"
            MATCH (n)
            OPTIONAL MATCH ()-[r]-(n)
            RETURN COLLECT(DISTINCT n) AS nodes, COLLECT(DISTINCT r) AS rels
        ");

        var list = await result.ToListAsync();
        var record = list[0];
        var nodesList = record["nodes"].As<List<INode>>();
        var relationsList = record["rels"].As<List<IRelationship>>();

        var nodesUnique = nodesList.DistinctBy(n => n.ElementId).ToArray();
        var relationsUnique = relationsList.DistinctBy(r => r.ElementId).ToArray();
        
        var nodeElementIdToIdDict = nodesUnique.ToDictionary(n => n.ElementId, n => Guid.Parse(n.Properties["id"].As<string>()));

        var historyEvents = nodesUnique.Select(n => new PureHistoryEvent()
        {
            Id = Guid.Parse(n.Properties["id"].As<string>()),
            Title = n.Properties["title"].As<string>(),
            Description = n.Properties["description"].As<string>(),
            Keywords = n.Properties["keywords"].As<List<string>>().ToArray(),
            TimeFrom = n.Properties["time_from"].As<ZonedDateTime>().UtcDateTime,
            TimeTo = n.Properties["time_to"].As<ZonedDateTime>().UtcDateTime
        }).ToArray();

        var relationships = relationsUnique.Select(r => new PureRelationWithIdsAndLabel()
        {
            Id = Guid.Parse(r.Properties["id"].As<string>()),
            FromId = nodeElementIdToIdDict[r.StartNodeElementId.As<string>()],
            ToId = nodeElementIdToIdDict[r.EndNodeElementId.As<string>()],
            Label = r.Type,
        }).ToArray();
        
        return new EventsAndRelationships(historyEvents, relationships);
    }
    
    public async Task<bool> AddEvents(PureHistoryEvent[] historyEvents, IAsyncQueryRunner queryRunner)
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

        var result = await queryRunner.RunAsync($@"
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

        var peek = await result.PeekAsync();
        var createdCount = (await result.SingleAsync());

        return true;
    }
    
    public async Task<bool> AddEvents(PureHistoryEvent[] historyEvents)
    {
        return await AddEvents(historyEvents, _session);
    }

    public class RelationsToAdd
    {
        public List<PureRelationContinue> Continues { get; set; } = [];
        public List<PureRelationPureInfluenced> Influenceds { get; set; } = [];
        public List<PureRelationPureReferences> Referencess { get; set; } = [];
        public List<PureRelationPureRelates> Relatess { get; set; } = [];

        public List<PureRelationPureRelatesTheme> ThemeRelatess { get; set; } = [];
    }

    public async Task<bool> AddRelations(RelationsToAdd relationsToAdd)
    {
        return await AddRelations(relationsToAdd, _session);
    }
    public async Task<bool> AddRelations(RelationsToAdd relationsToAdd, IAsyncQueryRunner queryRunner)
    {
        async Task<IResultCursor?> HandleRelations(
            IReadOnlyList<IPureRelationWithIds> relations,
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

            var result = await queryRunner.RunAsync(cypher, new { Rels = records });
            var resp = await result.ToListAsync();
            return result;
        }

        var r1 = await HandleRelations(
            relationsToAdd.Continues,
            PureRelationContinue.Label,
            PureRelationContinue.FromLabel,
            PureRelationContinue.ToLabel);

        var r2 = await HandleRelations(
            relationsToAdd.Influenceds,
            PureRelationPureInfluenced.Label,
            PureRelationPureInfluenced.FromLabel,
            PureRelationPureInfluenced.ToLabel);

        var r3 = await HandleRelations(
            relationsToAdd.Referencess,
            PureRelationPureReferences.Label,
            PureRelationPureReferences.FromLabel,
            PureRelationPureReferences.ToLabel);

        var r4 = await HandleRelations(
            relationsToAdd.Relatess,
            PureRelationPureRelates.Label,
            PureRelationPureRelates.FromLabel,
            PureRelationPureRelates.ToLabel);

        var r5 = await HandleRelations(
            relationsToAdd.ThemeRelatess,
            PureRelationPureRelatesTheme.Label,
            PureRelationPureRelatesTheme.FromLabel,
            PureRelationPureRelatesTheme.ToLabel);
        return true;
    }
}