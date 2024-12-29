using System;

namespace CardPool.Core;

/// <summary>
/// The rarity of the card.
/// </summary>
public enum CardRarity
{
    ZeroStar,
    OneStar,
    TwoStar,
    ThreeStar,
    FourStar,
    FiveStar,
    SixStar,
    SevenStar,
    EightStar,
    NineStar,
    TenStar
}
/// <summary>
/// Represents the various attributes that can be applied to a card.
/// </summary>
[Flags]
public enum CardAttributes
{
    /// <summary>
    /// No attributes are set.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates the card has a limited quantity available.
    /// </summary>
    Limited = 1 << 0,

    /// <summary>
    /// Indicates the card's real probability is fixed and won't be auto-adjusted.
    /// </summary>
    FixedRealProbability = 1 << 1,

    /// <summary>
    /// Indicates the card has been removed from the pool.
    /// </summary>
    Removed = 1 << 2,
}