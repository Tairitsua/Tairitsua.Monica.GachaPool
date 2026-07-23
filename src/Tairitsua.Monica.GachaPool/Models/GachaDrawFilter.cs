namespace Tairitsua.Monica.GachaPool.Models;

/// <summary>
/// Restricts a draw to a rarity tier or an include/exclude set of prize identifiers.
/// </summary>
public sealed class GachaDrawFilter
{
    private readonly HashSet<string>? _includedPrizeIds;
    private readonly HashSet<string> _excludedPrizeIds;

    private GachaDrawFilter(
        GachaRarity? rarity,
        IEnumerable<string>? includedPrizeIds,
        IEnumerable<string>? excludedPrizeIds)
    {
        Rarity = rarity;
        _includedPrizeIds = includedPrizeIds is null
            ? null
            : new HashSet<string>(includedPrizeIds.Select(NormalizeId), StringComparer.OrdinalIgnoreCase);
        _excludedPrizeIds = new HashSet<string>(
            (excludedPrizeIds ?? []).Select(NormalizeId),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a filter that accepts every currently available prize and preserves the pool's no-prize probability.
    /// </summary>
    public static GachaDrawFilter Any { get; } = new(null, null, null);

    /// <summary>
    /// Gets the required rarity, or <see langword="null"/> when all rarities are accepted.
    /// </summary>
    public GachaRarity? Rarity { get; }

    /// <summary>
    /// Creates a filter that accepts prizes from one rarity tier.
    /// </summary>
    /// <param name="rarity">The only accepted rarity.</param>
    /// <returns>A new immutable filter.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rarity"/> is undefined.</exception>
    public static GachaDrawFilter ForRarity(GachaRarity rarity)
    {
        if (!Enum.IsDefined(rarity))
        {
            throw new ArgumentOutOfRangeException(nameof(rarity), rarity, "Rarity must be a defined gacha rarity.");
        }

        return new GachaDrawFilter(rarity, null, null);
    }

    /// <summary>
    /// Creates a filter that accepts only the supplied prize identifiers.
    /// </summary>
    /// <param name="prizeIds">One or more prize identifiers.</param>
    /// <returns>A new immutable filter.</returns>
    public static GachaDrawFilter IncludeOnly(params string[] prizeIds)
    {
        return new GachaDrawFilter(null, RequirePrizeIds(prizeIds), null);
    }

    /// <summary>
    /// Creates a filter that rejects the supplied prize identifiers.
    /// </summary>
    /// <param name="prizeIds">One or more prize identifiers.</param>
    /// <returns>A new immutable filter.</returns>
    public static GachaDrawFilter Excluding(params string[] prizeIds)
    {
        return new GachaDrawFilter(null, null, RequirePrizeIds(prizeIds));
    }

    internal bool Matches<TPrize>(GachaPrizeDefinition<TPrize> prize) where TPrize : notnull
    {
        return (Rarity is null || prize.Rarity == Rarity)
               && (_includedPrizeIds is null || _includedPrizeIds.Contains(prize.Id))
               && !_excludedPrizeIds.Contains(prize.Id);
    }

    internal bool IsUnrestricted => Rarity is null && _includedPrizeIds is null && _excludedPrizeIds.Count == 0;

    private static string NormalizeId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return id.Trim();
    }

    private static string[] RequirePrizeIds(string[] prizeIds)
    {
        ArgumentNullException.ThrowIfNull(prizeIds);
        if (prizeIds.Length == 0)
        {
            throw new ArgumentException("At least one prize identifier is required.", nameof(prizeIds));
        }

        return prizeIds;
    }
}
