using AwesomeAssertions;
using Tairitsua.Monica.GachaPool.Models;

namespace Test.Tairitsua.Monica.GachaPool.Models;

public sealed class GachaDrawFilterTests
{
    [Fact]
    public void IncludeOnlyRejectsAnEmptyIdentifierSet()
    {
        Action action = () => GachaDrawFilter.IncludeOnly();

        action.Should()
            .Throw<ArgumentException>()
            .WithParameterName("prizeIds");
    }

    [Fact]
    public void ExcludingRejectsBlankIdentifiers()
    {
        Action action = () => GachaDrawFilter.Excluding("valid", " ");

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ForRarityRejectsUndefinedRarity()
    {
        Action action = () => GachaDrawFilter.ForRarity((GachaRarity)0);

        action.Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithParameterName("rarity");
    }
}
