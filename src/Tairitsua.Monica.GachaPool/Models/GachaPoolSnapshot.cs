namespace Tairitsua.Monica.GachaPool.Models;

/// <summary>
/// Captures a consistent view of one pool's configuration, inventory, and statistics.
/// </summary>
public sealed class GachaPoolSnapshot
{
    internal GachaPoolSnapshot(
        string id,
        string displayName,
        string? description,
        string prizeTypeName,
        double noPrizeProbability,
        long totalDraws,
        long noPrizeDraws,
        IReadOnlyList<GachaEntrySnapshot> entries,
        IReadOnlyList<GachaDrawResult> recentDraws)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
        PrizeTypeName = prizeTypeName;
        NoPrizeProbability = noPrizeProbability;
        TotalDraws = totalDraws;
        NoPrizeDraws = noPrizeDraws;
        Entries = entries;
        RecentDraws = recentDraws;
    }

    /// <summary>Gets the stable pool identifier.</summary>
    public string Id { get; }

    /// <summary>Gets the human-readable pool name.</summary>
    public string DisplayName { get; }

    /// <summary>Gets optional explanatory text.</summary>
    public string? Description { get; }

    /// <summary>Gets the fully qualified CLR name of the typed prize value represented by this pool.</summary>
    public string PrizeTypeName { get; }

    /// <summary>Gets the current probability of no prize for an unrestricted draw.</summary>
    public double NoPrizeProbability { get; }

    /// <summary>Gets the total number of draws since registration or reset.</summary>
    public long TotalDraws { get; }

    /// <summary>Gets how many draws produced no prize or an exhausted outcome.</summary>
    public long NoPrizeDraws { get; }

    /// <summary>Gets prize snapshots in deterministic registration order.</summary>
    public IReadOnlyList<GachaEntrySnapshot> Entries { get; }

    /// <summary>Gets the most recent draw outcomes, newest first.</summary>
    public IReadOnlyList<GachaDrawResult> RecentDraws { get; }

    /// <summary>Gets how many prizes are currently available.</summary>
    public int AvailablePrizeCount => Entries.Count(static entry => entry.IsAvailable);
}
