namespace Euynac.Monica.GachaPool.Models;

/// <summary>
/// Presents prize identity without exposing its publisher-owned typed value.
/// </summary>
public sealed class GachaPrizeSnapshot
{
    internal GachaPrizeSnapshot(string id, string displayName, string? description, GachaRarity rarity)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        Rarity = rarity;
    }

    /// <summary>Gets the stable prize identifier.</summary>
    public string Id { get; }

    /// <summary>Gets the human-readable prize name.</summary>
    public string DisplayName { get; }

    /// <summary>Gets optional explanatory text.</summary>
    public string? Description { get; }

    /// <summary>Gets the prize rarity.</summary>
    public GachaRarity Rarity { get; }
}
