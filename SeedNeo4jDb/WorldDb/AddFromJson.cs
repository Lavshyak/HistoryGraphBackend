using System.Text.Json;
using DbThings;
using DbThings.PureEntities;

namespace SeedNeo4jDb.WorldDb;

public record EventsJsonRoot(
    PureHistoryEventWithTemporarySlug[] Events,
    Relations Relations
);

public record Relations(
    List<PureRelationContinueWithTemporarySlugs> Continues,
    List<RelationPureInfluencedWithTemporarySlugs> Influenceds
);

public class AddFromJson
{
    private readonly EventsRepository _eventsRepository;
    private readonly MigrationsService _migrationsService;

    public AddFromJson(EventsRepository eventsRepository, MigrationsService migrationsService)
    {
        _eventsRepository = eventsRepository;
        _migrationsService = migrationsService;
    }

    public async Task ReadAndAdd()
    {
        string jsonFile = "events.json";
        var text = System.IO.File.ReadAllText(jsonFile);
        var root = JsonSerializer.Deserialize<EventsJsonRoot>(text);

        await _migrationsService.TryApplyMigrationToHistory("seed", "fake nodes and relationships",
            async transaction =>
            {
                await _eventsRepository.AddEvents(root.Events, transaction);

                var eventsSlugIdDict = root.Events.ToDictionary(e => e.TemporarySlug, e => e.Id);

                var relationsToAdd = new EventsRepository.RelationsToAdd()
                {
                    Continues = root.Relations.Continues.Select(c => new PureRelationContinue()
                    {
                        Id = Guid.Empty,
                        FromId = eventsSlugIdDict[c.FromTemporarySlug],
                        ToId = eventsSlugIdDict[c.ToTemporarySlug]
                    }).ToList(),
                    Influenceds = root.Relations.Influenceds.Select(c => new PureRelationPureInfluenced()
                    {
                        Id = Guid.Empty,
                        FromId = eventsSlugIdDict[c.FromTemporarySlug],
                        ToId = eventsSlugIdDict[c.ToTemporarySlug]
                    }).ToList(),
                };

                await _eventsRepository.AddRelations(relationsToAdd, transaction);
            }
        );
    }
}