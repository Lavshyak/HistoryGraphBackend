namespace DbThings.PureEntities;

public class PureHistoryEvent : IPureHistoryEvent
{
    public const string Label = "HISTORY_EVENT";
    public Guid Id { get; set; }
    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }
    public string[] Keywords { get; set; } = [];
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}