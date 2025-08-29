namespace DbThings.PureEntities;

public class PureRelationPureInfluenced : IPureRelationWithIds
{
    public const string Label = "INFLUENCED";
    public const string FromLabel = PureHistoryEvent.Label;
    public const string ToLabel = PureHistoryEvent.Label;
    
    public required Guid Id { get; set; }

    public required Guid FromId { get; init; }
    public required Guid ToId { get; init; }
}