namespace WebApi.Dtos;

public record DtoEventsAndRelationships(
    IEnumerable<DtoPureHistoryEvent> Events,
    IEnumerable<DtoPureRelationWithIdsAndLabel> Relationships)
{
}