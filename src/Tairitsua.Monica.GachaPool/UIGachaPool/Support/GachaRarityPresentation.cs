using Tairitsua.Monica.GachaPool.Localization;
using Tairitsua.Monica.GachaPool.Models;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace Tairitsua.Monica.GachaPool.UIGachaPool.Support;

internal static class GachaRarityPresentation
{
    internal static Color GetColor(GachaRarity rarity)
    {
        return rarity switch
        {
            >= GachaRarity.EightStar => Color.Error,
            >= GachaRarity.FiveStar => Color.Warning,
            >= GachaRarity.ThreeStar => Color.Tertiary,
            _ => Color.Info
        };
    }

    internal static string GetLabel(
        IStringLocalizer<GachaPoolResource> localizer,
        GachaRarity rarity)
    {
        return rarity switch
        {
            GachaRarity.OneStar => localizer["Rarity:OneStar"],
            GachaRarity.TwoStar => localizer["Rarity:TwoStar"],
            GachaRarity.ThreeStar => localizer["Rarity:ThreeStar"],
            GachaRarity.FourStar => localizer["Rarity:FourStar"],
            GachaRarity.FiveStar => localizer["Rarity:FiveStar"],
            GachaRarity.SixStar => localizer["Rarity:SixStar"],
            GachaRarity.SevenStar => localizer["Rarity:SevenStar"],
            GachaRarity.EightStar => localizer["Rarity:EightStar"],
            GachaRarity.NineStar => localizer["Rarity:NineStar"],
            GachaRarity.TenStar => localizer["Rarity:TenStar"],
            _ => rarity.ToString()
        };
    }
}
