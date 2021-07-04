using System;

namespace CardPool.Core
{
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
    [Flags]
    public enum CardAttributes
    {
        None = 0,
        Limited = 1 << 0,
        FixedRealProbability = 1 << 1,
        Removed = 1 << 2,
    }
}