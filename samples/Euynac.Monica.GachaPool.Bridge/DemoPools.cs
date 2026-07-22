using Euynac.Monica.GachaPool.Models;

namespace Euynac.Monica.GachaPool.Bridge;

internal static class DemoPools
{
    internal static GachaPoolDefinition<DemoReward> CreateStarlightPool()
    {
        return GachaPoolBuilder
            .Create<DemoReward>("starlight-standard", "Starlight Standard")
            .WithDescription("A balanced permanent pool with a finite five-star showcase reward.")
            .SetRarityProbability(GachaRarity.OneStar, 0.58)
            .SetRarityProbability(GachaRarity.TwoStar, 0.27)
            .SetRarityProbability(GachaRarity.ThreeStar, 0.11)
            .SetRarityProbability(GachaRarity.FiveStar, 0.04)
            .AddPrize(
                "moon-shard",
                "Moon Shard",
                new DemoReward("material.moon-shard", 10),
                GachaRarity.OneStar,
                weight: 3,
                description: "A common crafting fragment.")
            .AddPrize(
                "star-dust",
                "Star Dust",
                new DemoReward("material.star-dust", 25),
                GachaRarity.OneStar,
                weight: 2,
                description: "A luminous material for ascension.")
            .AddPrize(
                "silver-ticket",
                "Silver Ticket",
                new DemoReward("ticket.silver", 60),
                GachaRarity.TwoStar,
                weight: 2,
                description: "Exchangeable in the seasonal shop.")
            .AddPrize(
                "nebula-frame",
                "Nebula Frame",
                new DemoReward("cosmetic.nebula-frame", 140),
                GachaRarity.ThreeStar,
                description: "An animated profile frame.")
            .AddPrize(
                "aurora-companion",
                "Aurora Companion",
                new DemoReward("companion.aurora", 900),
                GachaRarity.FiveStar,
                initialStock: 3,
                description: "A limited showcase companion.")
            .Build();
    }

    internal static GachaPoolDefinition<DemoReward> CreateMidnightPool()
    {
        return GachaPoolBuilder
            .Create<DemoReward>("midnight-limited", "Midnight Limited")
            .WithDescription("A small event pool with a visible 2% no-prize interval and finite headline inventory.")
            .SetRarityProbability(GachaRarity.TwoStar, 0.63)
            .SetRarityProbability(GachaRarity.ThreeStar, 0.25)
            .SetRarityProbability(GachaRarity.FourStar, 0.08)
            .SetRarityProbability(GachaRarity.SixStar, 0.02)
            .AddPrize(
                "midnight-token",
                "Midnight Token",
                new DemoReward("currency.midnight", 40),
                GachaRarity.TwoStar,
                weight: 4,
                description: "Event currency for the Midnight exchange.")
            .AddPrize(
                "violet-sigil",
                "Violet Sigil",
                new DemoReward("material.violet-sigil", 110),
                GachaRarity.ThreeStar,
                weight: 2,
                description: "A rare upgrade catalyst.")
            .AddPrize(
                "eclipse-trail",
                "Eclipse Trail",
                new DemoReward("cosmetic.eclipse-trail", 320),
                GachaRarity.FourStar,
                initialStock: 8,
                description: "A limited movement trail.")
            .AddPrize(
                "nocturne-dragon",
                "Nocturne Dragon",
                new DemoReward("companion.nocturne-dragon", 1800),
                GachaRarity.SixStar,
                initialStock: 1,
                description: "The event's single-copy mythic reward.")
            .Build();
    }
}

internal sealed record DemoReward(string Sku, int CreditValue);
