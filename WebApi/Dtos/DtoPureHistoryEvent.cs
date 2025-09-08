using DbThings.PureEntities;

namespace WebApi.Dtos;

public record DtoPureHistoryEvent(
    Guid Id,
    DateTime TimeFrom,
    DateTime TimeTo,
    string[] Keywords,
    string Title,
    string Description)
{
    public static DtoPureHistoryEvent FromPureHistoryEvent(IPureHistoryEvent pureHistoryEvent)
    {
        return new DtoPureHistoryEvent(pureHistoryEvent.Id, pureHistoryEvent.TimeFrom, pureHistoryEvent.TimeTo,
            pureHistoryEvent.Keywords, pureHistoryEvent.Title, pureHistoryEvent.Description);
    }
}