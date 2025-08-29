namespace DbThings.PureEntities;

public class PureRelationPureRelatesTheme : IPureRelationWithIds
{
    public const string Label = "THEME_RELATES";
    public const string FromLabel = PureHistoryTheme.Label;
    public const string ToLabel = PureHistoryTheme.Label;
    public required Guid Id { get; set; }

    public required Guid FromId { get; init; }
    public required Guid ToId { get; init; }
}