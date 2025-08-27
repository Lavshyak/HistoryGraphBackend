using System.Text.Json;

namespace SanboxNeo.WorldDb;

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
    private EventsRepository _eventsRepository;

    public AddFromJson(EventsRepository eventsRepository)
    {
        _eventsRepository = eventsRepository;
    }

    public async Task ReadAndAdd()
    {
        string jsonFile = "events.json";
        var text = System.IO.File.ReadAllText(jsonFile);
        var root = JsonSerializer.Deserialize<EventsJsonRoot>(text);

        await _eventsRepository.AddEvents(root.Events);


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

        await _eventsRepository.AddRelations(relationsToAdd);
    }
}