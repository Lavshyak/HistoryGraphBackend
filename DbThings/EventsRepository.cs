using Neo4j.Driver;

namespace DbThings;

public class EventsRepository
{
    private readonly IAsyncSession _session;

    public EventsRepository(IAsyncSession session)
    {
        _session = session;
    }

    public async Task GetAll()
    {
        var result = await _session.RunAsync(@"
            MATCH (n)
            OPTIONAL MATCH (n)-[r]-(m)
            RETURN n, r, m;
        ");

        var list = await result.ToListAsync();
    }
    
    public async Task<bool> AddEvents(HistoryEvent[] historyEvents, IAsyncQueryRunner queryRunner)
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
    
    public async Task<bool> AddEvents(HistoryEvent[] historyEvents)
    {
        return await AddEvents(historyEvents, _session);
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
        return await AddRelations(relationsToAdd, _session);
    }
    public async Task<bool> AddRelations(RelationsToAdd relationsToAdd, IAsyncQueryRunner queryRunner)
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

            var result = await queryRunner.RunAsync(cypher, new { Rels = records });
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