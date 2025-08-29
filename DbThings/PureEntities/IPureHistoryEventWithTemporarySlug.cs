namespace DbThings.PureEntities;

public interface IPureHistoryEventWithTemporarySlug
{
    public string TemporarySlug { get; init; }
}