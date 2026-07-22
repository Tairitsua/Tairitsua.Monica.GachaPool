namespace Euynac.Monica.GachaPool.Models;

/// <summary>
/// Describes the live probability, inventory, and draw statistics of one prize.
/// </summary>
public sealed class GachaEntrySnapshot
{
    internal GachaEntrySnapshot(
        GachaPrizeSnapshot prize,
        double weight,
        double probability,
        int? initialStock,
        int? remainingStock,
        long drawCount,
        double observedProbability)
    {
        Prize = prize;
        Weight = weight;
        Probability = probability;
        InitialStock = initialStock;
        RemainingStock = remainingStock;
        DrawCount = drawCount;
        ObservedProbability = observedProbability;
    }

    /// <summary>Gets the prize identity.</summary>
    public GachaPrizeSnapshot Prize { get; }

    /// <summary>Gets the configured relative weight within the prize's rarity tier.</summary>
    public double Weight { get; }

    /// <summary>Gets the prize's current effective probability for an unrestricted draw.</summary>
    public double Probability { get; }

    /// <summary>Gets the initial finite stock, or <see langword="null"/> for unlimited stock.</summary>
    public int? InitialStock { get; }

    /// <summary>Gets the current finite stock, or <see langword="null"/> for unlimited stock.</summary>
    public int? RemainingStock { get; }

    /// <summary>Gets how many tracked draws selected this prize.</summary>
    public long DrawCount { get; }

    /// <summary>Gets the prize's observed share of all tracked draws.</summary>
    public double ObservedProbability { get; }

    /// <summary>Gets whether the prize can currently be drawn.</summary>
    public bool IsAvailable => RemainingStock is null or > 0;
}
