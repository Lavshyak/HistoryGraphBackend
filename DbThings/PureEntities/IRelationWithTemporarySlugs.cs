namespace DbThings.PureEntities;

public interface IRelationWithTemporarySlugs
{
    public string FromTemporarySlug { get; }
    public string ToTemporarySlug { get; }
}