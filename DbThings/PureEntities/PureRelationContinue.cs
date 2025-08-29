namespace DbThings.PureEntities;

public class PureRelationContinue : IPureRelationWithIds
{
    public const string Label = "CONTINUE";
    public const string FromLabel = PureHistoryEvent.Label;
    public const string ToLabel = PureHistoryEvent.Label;
    
    public required Guid Id { get; set; }
    
    public required Guid FromId { get; init; }

    public required Guid ToId { get; init; }
}