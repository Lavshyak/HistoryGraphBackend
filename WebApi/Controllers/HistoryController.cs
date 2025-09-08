using System.Text.Json;
using DbThings;
using DbThings.PureEntities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class HistoryController : ControllerBase
{
    private readonly ILogger<HistoryController> _logger;
    private readonly EventsRepository _eventsRepository;

    public HistoryController(ILogger<HistoryController> logger, EventsRepository eventsRepository)
    {
        _logger = logger;
        _eventsRepository = eventsRepository;
    }

    [HttpGet]
    public async Task<DtoEventsAndRelationships> GetAll()
    {
        var getAllResult = await _eventsRepository.GetAll();

        var events = getAllResult.Events.Select(DtoPureHistoryEvent.FromPureHistoryEvent);
        var rels = getAllResult.Relations.Select(DtoPureRelationWithIdsAndLabel.FromPureRelationWithIdsAndLabel);
        var result = new DtoEventsAndRelationships(events, rels);
        return result;
    }

    public record AddNodesAndEdgesInput(PureHistoryEvent[] PureHistoryEvents, EventsRepository.RelationsToAdd RelationsToAdd);
    
    [HttpPost]
    public async Task<Ok> AddNodesAndEdges(AddNodesAndEdgesInput nodesAndEdgesInput)
    {
        _logger.LogDebug(JsonSerializer.Serialize(nodesAndEdgesInput));
        var transaction = await _eventsRepository.BeginTransactionAsync();
        await _eventsRepository.AddEvents(nodesAndEdgesInput.PureHistoryEvents, transaction);
        await _eventsRepository.AddRelations(nodesAndEdgesInput.RelationsToAdd, transaction);
        await transaction.CommitAsync();
        return TypedResults.Ok();
    }
}