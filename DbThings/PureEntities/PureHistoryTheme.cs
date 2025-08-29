namespace DbThings.PureEntities;

public class PureHistoryTheme : IHaveIdGuid
{
    public const string Label = "HISTORY_THEME";

    public Guid Id { get; set; }
    public string Keywords { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}