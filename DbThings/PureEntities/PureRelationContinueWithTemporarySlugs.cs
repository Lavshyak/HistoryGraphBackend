namespace DbThings.PureEntities;

public class PureRelationContinueWithTemporarySlugs : IRelationWithTemporarySlugs
{
    public required string FromTemporarySlug { get; set; }
    public required string ToTemporarySlug { get; set; }
}