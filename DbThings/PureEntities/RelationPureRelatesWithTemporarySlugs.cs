namespace DbThings.PureEntities;

public class RelationPureRelatesWithTemporarySlugs : IRelationWithTemporarySlugs
{
    public required string FromTemporarySlug { get; set; }
    public required string ToTemporarySlug { get; set; }
}