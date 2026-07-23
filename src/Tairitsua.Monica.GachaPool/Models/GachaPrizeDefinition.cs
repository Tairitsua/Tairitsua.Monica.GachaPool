namespace Tairitsua.Monica.GachaPool.Models;

/// <summary>
/// Defines one typed prize and its weighting within a rarity tier.
/// </summary>
/// <typeparam name="TPrize">The publisher-owned value delivered when this prize is drawn.</typeparam>
public sealed class GachaPrizeDefinition<TPrize> where TPrize : notnull
{
    internal GachaPrizeDefinition(
        string id,
        string displayName,
        string? description,
        TPrize value,
        GachaRarity rarity,
        double weight,
        int? initialStock)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        Value = value;
        Rarity = rarity;
        Weight = weight;
        InitialStock = initialStock;
    }

    /// <summary>
    /// Gets the stable prize identifier, unique within its pool.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the human-readable prize name used by diagnostics and the bundled UI.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets optional explanatory text for the prize.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the publisher-owned typed value returned to typed callers.
    /// </summary>
    public TPrize Value { get; }

    /// <summary>
    /// Gets the rarity group whose configured probability owns this prize.
    /// </summary>
    public GachaRarity Rarity { get; }

    /// <summary>
    /// Gets the relative weight of this prize among currently available prizes of the same rarity.
    /// </summary>
    public double Weight { get; }

    /// <summary>
    /// Gets the initial finite stock, or <see langword="null"/> when the prize is unlimited.
    /// </summary>
    public int? InitialStock { get; }
}
