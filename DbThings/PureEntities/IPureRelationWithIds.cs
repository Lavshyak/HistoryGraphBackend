namespace DbThings.PureEntities;

public interface IPureRelationWithIds : IHaveIdGuid
{
    public Guid FromId { get; }
    public Guid ToId { get; }
}