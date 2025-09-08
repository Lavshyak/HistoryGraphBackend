using DbThings.PureEntities;

namespace WebApi.Dtos;

public record DtoPureRelationWithIdsAndLabel(Guid Id,
    Guid FromId,
    Guid ToId,
    string Label)
{
    public static DtoPureRelationWithIdsAndLabel FromPureRelationWithIdsAndLabel(IPureRelationWithIdsAndLabel pureRelationWithIdsAndLabel)
    {
        return new DtoPureRelationWithIdsAndLabel(
            pureRelationWithIdsAndLabel.Id,
            pureRelationWithIdsAndLabel.FromId,
            pureRelationWithIdsAndLabel.ToId,
            pureRelationWithIdsAndLabel.Label
        );
    }
}