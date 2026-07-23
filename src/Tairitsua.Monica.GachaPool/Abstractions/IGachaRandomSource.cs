namespace Tairitsua.Monica.GachaPool.Abstractions;

/// <summary>
/// Supplies independent random values to gacha pool runtimes.
/// </summary>
/// <remarks>
/// Implementations may be singleton and must therefore be thread-safe. Values must be finite and belong to the
/// half-open interval [0, 1).
/// </remarks>
public interface IGachaRandomSource
{
    /// <summary>
    /// Returns the next random value in the half-open interval [0, 1).
    /// </summary>
    /// <returns>A finite value greater than or equal to zero and less than one.</returns>
    double NextUnitInterval();
}
