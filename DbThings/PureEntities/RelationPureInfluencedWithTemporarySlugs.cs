namespace DbThings.PureEntities;

public class RelationPureInfluencedWithTemporarySlugs : IRelationWithTemporarySlugs
{
    public required string FromTemporarySlug { get; set; }
    public required string ToTemporarySlug { get; set; }
}