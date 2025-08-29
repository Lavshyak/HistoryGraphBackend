namespace DbThings.PureEntities;

public class RelationPureReferencesWithTemporarySlugs : IRelationWithTemporarySlugs
{
    public required string FromTemporarySlug { get; set; }
    public required string ToTemporarySlug { get; set; }
}