namespace Tairitsua.Monica.GachaPool.Models;

/// <summary>
/// Creates typed builders for immutable gacha pool definitions.
/// </summary>
public static class GachaPoolBuilder
{
    /// <summary>
    /// Starts a typed pool definition.
    /// </summary>
    /// <typeparam name="TPrize">The publisher-owned prize value type.</typeparam>
    /// <param name="id">Stable identifier unique within one catalog.</param>
    /// <param name="displayName">Human-readable name used by diagnostics and UI.</param>
    /// <returns>A fluent pool builder.</returns>
    public static GachaPoolBuilder<TPrize> Create<TPrize>(string id, string displayName)
        where TPrize : notnull
    {
        return new GachaPoolBuilder<TPrize>(id, displayName);
    }
}

/// <summary>
/// Builds a typed gacha pool with explicit rarity probabilities and within-rarity prize weights.
/// </summary>
/// <typeparam name="TPrize">The publisher-owned prize value type.</typeparam>
public sealed class GachaPoolBuilder<TPrize> where TPrize : notnull
{
    private const double PROBABILITY_TOLERANCE = 1e-12;

    private readonly string _id;
    private readonly string _displayName;
    private readonly List<GachaPrizeDefinition<TPrize>> _prizes = [];
    private readonly Dictionary<GachaRarity, double> _rarityProbabilities = [];
    private string? _description;

    internal GachaPoolBuilder(string id, string displayName)
    {
        _id = RequireText(id, nameof(id));
        _displayName = RequireText(displayName, nameof(displayName));
    }

    /// <summary>
    /// Sets optional explanatory text for the pool.
    /// </summary>
    /// <param name="description">Description displayed to operators, or <see langword="null"/> to omit it.</param>
    /// <returns>The current builder.</returns>
    public GachaPoolBuilder<TPrize> WithDescription(string? description)
    {
        _description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        return this;
    }

    /// <summary>
    /// Assigns an absolute probability to a rarity tier.
    /// </summary>
    /// <remarks>
    /// Probabilities for all configured tiers may total less than one; the remainder becomes an intentional no-prize
    /// outcome. A tier containing prizes must be configured before <see cref="Build"/> is called.
    /// </remarks>
    /// <param name="rarity">The rarity tier to configure.</param>
    /// <param name="probability">A finite probability greater than zero and at most one.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="rarity"/> is undefined, or <paramref name="probability"/> is not in the supported range.
    /// </exception>
    public GachaPoolBuilder<TPrize> SetRarityProbability(GachaRarity rarity, double probability)
    {
        ValidateRarity(rarity);

        if (!double.IsFinite(probability) || probability <= 0 || probability > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(probability),
                probability,
                "Rarity probability must be finite, greater than zero, and at most one.");
        }

        _rarityProbabilities[rarity] = probability;
        return this;
    }

    /// <summary>
    /// Adds a prize to the pool.
    /// </summary>
    /// <param name="id">Stable identifier unique within this pool.</param>
    /// <param name="displayName">Human-readable prize name.</param>
    /// <param name="value">Typed value delivered by typed draws.</param>
    /// <param name="rarity">Rarity tier that owns this prize's probability.</param>
    /// <param name="weight">Positive relative weight among available prizes of the same rarity.</param>
    /// <param name="initialStock">Positive finite stock, or <see langword="null"/> for unlimited stock.</param>
    /// <param name="description">Optional explanatory text.</param>
    /// <returns>The current builder.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="rarity"/> is undefined, <paramref name="weight"/> is not finite and positive, or finite stock is
    /// not positive.
    /// </exception>
    public GachaPoolBuilder<TPrize> AddPrize(
        string id,
        string displayName,
        TPrize value,
        GachaRarity rarity,
        double weight = 1,
        int? initialStock = null,
        string? description = null)
    {
        id = RequireText(id, nameof(id));
        displayName = RequireText(displayName, nameof(displayName));
        ArgumentNullException.ThrowIfNull(value);
        ValidateRarity(rarity);

        if (_prizes.Any(prize => string.Equals(prize.Id, id, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Prize identifier '{id}' is already registered in pool '{_id}'.");
        }

        if (!double.IsFinite(weight) || weight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), weight, "Prize weight must be finite and positive.");
        }

        if (initialStock is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialStock),
                initialStock,
                "Finite prize stock must be greater than zero.");
        }

        _prizes.Add(new GachaPrizeDefinition<TPrize>(
            id,
            displayName,
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            value,
            rarity,
            weight,
            initialStock));
        return this;
    }

    /// <summary>
    /// Validates and freezes the current configuration.
    /// </summary>
    /// <returns>An immutable pool definition safe to register with multiple Monica hosts.</returns>
    /// <exception cref="InvalidOperationException">
    /// The pool is empty, its rarity/prize mappings are incomplete, probabilities exceed 100%, or the total weight of
    /// a rarity cannot be represented as a finite <see cref="double"/>.
    /// </exception>
    public GachaPoolDefinition<TPrize> Build()
    {
        if (_prizes.Count == 0)
        {
            throw new InvalidOperationException($"Pool '{_id}' must contain at least one prize.");
        }

        var missingRarities = _prizes
            .Select(static prize => prize.Rarity)
            .Distinct()
            .Where(rarity => !_rarityProbabilities.ContainsKey(rarity))
            .Order()
            .ToArray();
        if (missingRarities.Length > 0)
        {
            throw new InvalidOperationException(
                $"Pool '{_id}' has prizes in rarity tiers without probabilities: {string.Join(", ", missingRarities)}.");
        }

        var emptyRarities = _rarityProbabilities.Keys
            .Where(rarity => _prizes.All(prize => prize.Rarity != rarity))
            .Order()
            .ToArray();
        if (emptyRarities.Length > 0)
        {
            throw new InvalidOperationException(
                $"Pool '{_id}' assigns probabilities to rarity tiers without prizes: {string.Join(", ", emptyRarities)}.");
        }

        var nonFiniteWeightRarities = _prizes
            .GroupBy(static prize => prize.Rarity)
            .Where(static group => !double.IsFinite(group.Sum(static prize => prize.Weight)))
            .Select(static group => group.Key)
            .Order()
            .ToArray();
        if (nonFiniteWeightRarities.Length > 0)
        {
            throw new InvalidOperationException(
                $"Pool '{_id}' has rarity tiers whose total prize weight is not finite: {string.Join(", ", nonFiniteWeightRarities)}.");
        }

        var totalProbability = _rarityProbabilities.Values.Sum();
        if (totalProbability > 1 + PROBABILITY_TOLERANCE)
        {
            throw new InvalidOperationException(
                $"Pool '{_id}' rarity probabilities total {totalProbability:P6}, which exceeds 100%.");
        }

        return new GachaPoolDefinition<TPrize>(
            _id,
            _displayName,
            _description,
            _prizes,
            _rarityProbabilities);
    }

    private static string RequireText(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.Trim();
    }

    private static void ValidateRarity(GachaRarity rarity)
    {
        if (!Enum.IsDefined(rarity))
        {
            throw new ArgumentOutOfRangeException(nameof(rarity), rarity, "Rarity must be a defined gacha rarity.");
        }
    }
}
