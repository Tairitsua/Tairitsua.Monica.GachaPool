using System;

namespace MoLibrary.GachaPool.Conventions;

/// <summary>
/// The rarity of the card.
/// </summary>
[Flags]
public enum CardRarity
{
    ZeroStar = 1 << 0,
    OneStar = 1 << 1,
    TwoStar = 1 << 2,
    ThreeStar = 1 << 3,
    FourStar = 1 << 4,
    FiveStar = 1 << 5,
    SixStar = 1 << 6,
    SevenStar = 1 << 7,
    EightStar = 1 << 8,
    NineStar = 1 << 9,
    TenStar = 1 << 10
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