namespace DbThings.PureEntities;

public interface IPureHistoryEvent : IHaveIdGuid
{
    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }
    public string[] Keywords { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}