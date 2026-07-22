namespace Euynac.Monica.GachaPool.Models;

/// <summary>
/// Describes the observable outcome of a draw.
/// </summary>
public enum GachaDrawOutcomeKind
{
    /// <summary>The draw selected a prize.</summary>
    Prize = 0,

    /// <summary>The draw landed in the pool's intentionally unassigned probability.</summary>
    NoPrize = 1,

    /// <summary>No prize matched the requested filter or all matching limited prizes were exhausted.</summary>
    Exhausted = 2
}
