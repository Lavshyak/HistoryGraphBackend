namespace DbThings.PureEntities;

public interface IPureRelationWithIdsAndLabel : IPureRelationWithIds
{
    public string Label { get; }
}