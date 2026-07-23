using System.Diagnostics.CodeAnalysis;

namespace Tairitsua.Monica.GachaPool.Models;

/// <summary>
/// Represents a presentation-safe draw outcome.
/// </summary>
public class GachaDrawResult
{
    internal GachaDrawResult(
        string poolId,
        long sequence,
        DateTimeOffset drawnAtUtc,
        GachaDrawOutcomeKind outcome,
        GachaPrizeSnapshot? prize,
        double probabilityAtDraw,
        int? remainingStock)
    {
        PoolId = poolId;
        Sequence = sequence;
        DrawnAtUtc = drawnAtUtc;
        Outcome = outcome;
        Prize = prize;
        ProbabilityAtDraw = probabilityAtDraw;
        RemainingStock = remainingStock;
    }

    /// <summary>Gets the pool identifier.</summary>
    public string PoolId { get; }

    /// <summary>Gets the monotonically increasing draw sequence within the current pool lifetime.</summary>
    public long Sequence { get; }

    /// <summary>Gets the UTC time at which the draw completed.</summary>
    public DateTimeOffset DrawnAtUtc { get; }

    /// <summary>Gets the observable outcome kind.</summary>
    public GachaDrawOutcomeKind Outcome { get; }

    /// <summary>Gets the drawn prize, or <see langword="null"/> for no-prize and exhausted outcomes.</summary>
    public GachaPrizeSnapshot? Prize { get; }

    /// <summary>Gets the effective probability of the selected outcome for this draw.</summary>
    public double ProbabilityAtDraw { get; }

    /// <summary>Gets remaining finite stock after the draw, or <see langword="null"/> for unlimited or absent prizes.</summary>
    public int? RemainingStock { get; }

    /// <summary>Gets whether this outcome delivered a prize.</summary>
    public bool HasPrize => Outcome == GachaDrawOutcomeKind.Prize;
}

/// <summary>
/// Represents a draw outcome that also carries the publisher-owned typed prize value.
/// </summary>
/// <typeparam name="TPrize">The publisher-owned prize value type.</typeparam>
public sealed class GachaDrawResult<TPrize> : GachaDrawResult where TPrize : notnull
{
    private readonly TPrize _value;

    internal GachaDrawResult(
        string poolId,
        long sequence,
        DateTimeOffset drawnAtUtc,
        GachaDrawOutcomeKind outcome,
        GachaPrizeSnapshot? prize,
        TPrize value,
        double probabilityAtDraw,
        int? remainingStock)
        : base(poolId, sequence, drawnAtUtc, outcome, prize, probabilityAtDraw, remainingStock)
    {
        _value = value;
    }

    /// <summary>Gets the typed prize value.</summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when this outcome did not deliver a prize. Check <see cref="GachaDrawResult.HasPrize"/> or call
    /// <see cref="TryGetValue"/> before accessing this property.
    /// </exception>
    public TPrize Value => HasPrize
        ? _value
        : throw new InvalidOperationException("A no-prize or exhausted draw does not carry a prize value.");

    /// <summary>
    /// Attempts to get the typed prize value without throwing for no-prize or exhausted outcomes.
    /// </summary>
    /// <param name="value">
    /// The typed prize value when the method returns <see langword="true"/>; otherwise the default value of
    /// <typeparamref name="TPrize"/>.
    /// </param>
    /// <returns><see langword="true"/> when this outcome delivered a prize; otherwise <see langword="false"/>.</returns>
    public bool TryGetValue([MaybeNullWhen(false)] out TPrize value)
    {
        if (HasPrize)
        {
            value = _value;
            return true;
        }

        value = default;
        return false;
    }

    internal GachaDrawResult ToPresentationResult()
    {
        return new GachaDrawResult(
            PoolId,
            Sequence,
            DrawnAtUtc,
            Outcome,
            Prize,
            ProbabilityAtDraw,
            RemainingStock);
    }
}
