using System.Collections.ObjectModel;

namespace Tairitsua.Monica.GachaPool.Models;

/// <summary>
/// Represents an immutable, validated definition from which a runtime gacha pool is created.
/// </summary>
/// <typeparam name="TPrize">The publisher-owned prize value type.</typeparam>
public sealed class GachaPoolDefinition<TPrize> where TPrize : notnull
{
    internal GachaPoolDefinition(
        string id,
        string displayName,
        string? description,
        IReadOnlyList<GachaPrizeDefinition<TPrize>> prizes,
        IReadOnlyDictionary<GachaRarity, double> rarityProbabilities)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        Prizes = Array.AsReadOnly(prizes.ToArray());
        RarityProbabilities = new ReadOnlyDictionary<GachaRarity, double>(
            new Dictionary<GachaRarity, double>(rarityProbabilities));
        NoPrizeProbability = Math.Max(0, 1 - rarityProbabilities.Values.Sum());
    }

    /// <summary>
    /// Gets the stable pool identifier used by the catalog and Facade.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the human-readable pool name displayed by the bundled UI.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets optional explanatory text for the pool.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the prizes in deterministic registration order.
    /// </summary>
    public IReadOnlyList<GachaPrizeDefinition<TPrize>> Prizes { get; }

    /// <summary>
    /// Gets each rarity tier's absolute probability in the unfiltered pool.
    /// </summary>
    public IReadOnlyDictionary<GachaRarity, double> RarityProbabilities { get; }

    /// <summary>
    /// Gets the unassigned probability that intentionally produces a no-prize outcome.
    /// </summary>
    public double NoPrizeProbability { get; }
}
