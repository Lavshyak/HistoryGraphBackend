namespace DbThings.PureEntities;

public class PureHistoryEventWithTemporarySlug : PureHistoryEvent, IPureHistoryEventWithTemporarySlug
{
    public required string TemporarySlug { get; init; }
}