using System.Text.Json;
using DbThings;

namespace SeedNeo4jDb.WorldDb;

public record EventsJsonRoot(
    HistoryEventWithTemporarySlug[] Events,
    Relations Relations
);

public record Relations(
    List<RelationPureContinueWithTemporarySlugs> Continues,
    List<RelationPureInfluencedWithTemporarySlugs> Influenceds
);

public class AddFromJson
{
    private readonly EventsRepository _eventsRepository;
    private readonly DataBase _dataBase;

    public AddFromJson(EventsRepository eventsRepository, DataBase dataBase)
    {
        _eventsRepository = eventsRepository;
        _dataBase = dataBase;
    }

    public async Task ReadAndAdd()
    {
        string jsonFile = "events.json";
        var text = System.IO.File.ReadAllText(jsonFile);
        var root = JsonSerializer.Deserialize<EventsJsonRoot>(text);

        await _dataBase.TryApplyMigration("initial_events", "",
            async transaction => { await _eventsRepository.AddEvents(root.Events, transaction); });

        var eventsSlugIdDict = root.Events.ToDictionary(e => e.TemporarySlug, e => e.Id);

        var relationsToAdd = new EventsRepository.RelationsToAdd()
        {
            Continues = root.Relations.Continues.Select(c => new RelationPureContinue()
            {
                Id = Guid.Empty,
                FromId = eventsSlugIdDict[c.FromTemporarySlug],
                ToId = eventsSlugIdDict[c.ToTemporarySlug]
            }).ToList(),
            Influenceds = root.Relations.Continues.Select(c => new RelationPureInfluenced()
            {
                Id = Guid.Empty,
                FromId = eventsSlugIdDict[c.FromTemporarySlug],
                ToId = eventsSlugIdDict[c.ToTemporarySlug]
            }).ToList(),
        };

        await _dataBase.TryApplyMigration("initial_relations", "",
            async transaction => { await _eventsRepository.AddRelations(relationsToAdd, transaction); });
    }
}