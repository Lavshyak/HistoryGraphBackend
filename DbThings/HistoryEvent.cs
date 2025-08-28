namespace DbThings;

public class HistoryEvent
{
    public const string Label = "HISTORY_EVENT";
    public Guid Id { get; set; }
    public string IdStr => Id.ToString();
    public DateTime TimeFrom { get; set; }
    public DateTime TimeTo { get; set; }
    public string[] Keywords { get; set; } = [];
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public RelationPureContinue[] Continues { get; set; } = [];
    public RelationPureInfluenced[] Influenceds { get; set; } = [];
    public RelationPureReferences[] Referencess { get; set; } = [];
    public RelationPureRelates[] Relatess { get; set; } = [];
}

public class HistoryEventWithTemporarySlug : HistoryEvent
{
    public required string TemporarySlug { get; init; }
}

public class HistoryTheme
{
    public const string Label = "HISTORY_THEMET";

    public Guid Id { get; set; }
    public string Keywords { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public RelationPureRelatesTheme[] Relateds { get; set; } = [];
}

public interface IRelationWithIds
{
    public Guid Id { get; set; }
    public string IdStr => Id.ToString();
    public Guid FromId { get; }
    public string FromIdStr => FromId.ToString();
    public Guid ToId { get; }
    public string ToIdStr => ToId.ToString();
}

public interface IRelationWithTemporarySlugs
{
    public string FromTemporarySlug { get; }
    public string ToTemporarySlug { get; }
}


public class RelationPureContinue : IRelationWithIds
{
    public const string Label = "CONTINUE";
    public const string FromLabel = HistoryEvent.Label;
    public const string ToLabel = HistoryEvent.Label;
    
    public required Guid Id { get; set; }
    
    public required Guid FromId { get; init; }

    public required Guid ToId { get; init; }
}

public class RelationPureContinueWithTemporarySlugs : IRelationWithTemporarySlugs
{
    public required string FromTemporarySlug { get; set; }
    public required string ToTemporarySlug { get; set; }
}

public class RelationPureInfluenced : IRelationWithIds
{
    public const string Label = "INFLUENCED";
    public const string FromLabel = HistoryEvent.Label;
    public const string ToLabel = HistoryEvent.Label;
    
    public required Guid Id { get; set; }

    public required Guid FromId { get; init; }
    public required Guid ToId { get; init; }
}

public class RelationPureInfluencedWithTemporarySlugs : IRelationWithTemporarySlugs
{
    public required string FromTemporarySlug { get; set; }
    public required string ToTemporarySlug { get; set; }
}

public class RelationPureReferences : IRelationWithIds
{
    public const string Label = "REFERENCES";
    public const string FromLabel = HistoryEvent.Label;
    public const string ToLabel = HistoryEvent.Label;
    public required Guid Id { get; set; }

    public required Guid FromId { get; init; }
    public required Guid ToId { get; init; }
}

public class RelationPureReferencesWithTemporarySlugs : IRelationWithTemporarySlugs
{
    public required string FromTemporarySlug { get; set; }
    public required string ToTemporarySlug { get; set; }
}

public class RelationPureRelates : IRelationWithIds
{
    public const string Label = "RELATES";
    public const string FromLabel = HistoryEvent.Label;
    public const string ToLabel = HistoryTheme.Label;
    public required Guid Id { get; set; }

    public required Guid FromId { get; init; }
    public required Guid ToId { get; init; }
}

public class RelationPureRelatesWithTemporarySlugs : IRelationWithTemporarySlugs
{
    public required string FromTemporarySlug { get; set; }
    public required string ToTemporarySlug { get; set; }
}

public class RelationPureRelatesTheme : IRelationWithIds
{
    public const string Label = "THEME_RELATES";
    public const string FromLabel = HistoryTheme.Label;
    public const string ToLabel = HistoryTheme.Label;
    public required Guid Id { get; set; }

    public required Guid FromId { get; init; }
    public required Guid ToId { get; init; }
}

public class RelationPureRelatesThemeWithTemporarySlugs : IRelationWithTemporarySlugs
{
    public required string FromTemporarySlug { get; set; }
    public required string ToTemporarySlug { get; set; }
}