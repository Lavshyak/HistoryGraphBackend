namespace DbThings.PureEntities;

public class PureRelationWithIdsAndLabel : IPureRelationWithIdsAndLabel
{
    public required Guid Id { get; set; }
    public required Guid FromId { get; init; }
    public required Guid ToId { get; init; }

    public required string Label { get; init; }
}