namespace DbThings.PureEntities;

public class PureRelationPureRelates : IPureRelationWithIds
{
    public const string Label = "RELATES";
    public const string FromLabel = PureHistoryEvent.Label;
    public const string ToLabel = PureHistoryTheme.Label;
    public required Guid Id { get; set; }

    public required Guid FromId { get; init; }
    public required Guid ToId { get; init; }
}