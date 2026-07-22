using AwesomeAssertions;
using Euynac.Monica.GachaPool.Models;

namespace Test.Euynac.Monica.GachaPool.Models;

public sealed class GachaPoolBuilderTests
{
    [Fact]
    public void BuildRejectsDefinitionWhenRarityProbabilityIsMissing()
    {
        var builder = GachaPoolBuilder
            .Create<string>("test", "Test")
            .AddPrize("prize", "Prize", "value", GachaRarity.ThreeStar);

        Action action = () => builder.Build();

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*ThreeStar*");
    }

    [Fact]
    public void BuildRejectsDefinitionWhenProbabilitiesExceedOne()
    {
        var builder = GachaPoolBuilder
            .Create<string>("test", "Test")
            .SetRarityProbability(GachaRarity.OneStar, 0.7)
            .SetRarityProbability(GachaRarity.TwoStar, 0.4)
            .AddPrize("one", "One", "one", GachaRarity.OneStar)
            .AddPrize("two", "Two", "two", GachaRarity.TwoStar);

        Action action = () => builder.Build();

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*exceeds 100%*");
    }

    [Fact]
    public void BuildFreezesNoPrizeProbabilityWhenConfigurationIsValid()
    {
        var definition = GachaPoolBuilder
            .Create<string>("test", "Test")
            .SetRarityProbability(GachaRarity.OneStar, 0.8)
            .AddPrize("one", "One", "one", GachaRarity.OneStar)
            .Build();

        definition.NoPrizeProbability.Should().BeApproximately(0.2, 1e-12);
        definition.Prizes.Should().ContainSingle();
    }

    [Fact]
    public void BuildRejectsDefinitionWhenRarityHasNoPrizes()
    {
        var builder = GachaPoolBuilder
            .Create<string>("test", "Test")
            .SetRarityProbability(GachaRarity.OneStar, 0.5)
            .SetRarityProbability(GachaRarity.TwoStar, 0.5)
            .AddPrize("one", "One", "one", GachaRarity.OneStar);

        Action action = () => builder.Build();

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*TwoStar*");
    }

    [Fact]
    public void BuildRejectsDefinitionWhenRarityWeightTotalIsNotFinite()
    {
        var builder = GachaPoolBuilder
            .Create<string>("test", "Test")
            .SetRarityProbability(GachaRarity.OneStar, 1)
            .AddPrize("one", "One", "one", GachaRarity.OneStar, double.MaxValue)
            .AddPrize("two", "Two", "two", GachaRarity.OneStar, double.MaxValue);

        Action action = () => builder.Build();

        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*total prize weight is not finite*OneStar*");
    }

    [Fact]
    public void SetRarityProbabilityRejectsUndefinedRarity()
    {
        var builder = GachaPoolBuilder.Create<string>("test", "Test");

        Action action = () => builder.SetRarityProbability((GachaRarity)0, 1);

        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("rarity");
    }

    [Fact]
    public void AddPrizeRejectsNullReferenceValue()
    {
        var builder = GachaPoolBuilder
            .Create<string>("test", "Test")
            .SetRarityProbability(GachaRarity.OneStar, 1);

        Action action = () => builder.AddPrize("one", "One", null!, GachaRarity.OneStar);

        action.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName("value");
    }
}
