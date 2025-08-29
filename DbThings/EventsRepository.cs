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

    public async Task GetAll()
    {
        var result = await _session.RunAsync(@"
            MATCH (n)
            OPTIONAL MATCH (n)-[r]-(m)
            RETURN n, r, m;
        ");

        var list = await result.ToListAsync();
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