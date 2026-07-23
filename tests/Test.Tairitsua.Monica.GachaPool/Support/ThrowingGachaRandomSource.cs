using Tairitsua.Monica.GachaPool.Abstractions;

namespace Test.Tairitsua.Monica.GachaPool.Support;

internal sealed class ThrowingGachaRandomSource : IGachaRandomSource
{
    internal const string DiagnosticToken = "sensitive-diagnostic-token";

    public double NextUnitInterval()
    {
        throw new InvalidOperationException(DiagnosticToken);
    }
}
