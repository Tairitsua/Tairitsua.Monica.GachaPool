using Euynac.Monica.GachaPool.Abstractions;

namespace Test.Euynac.Monica.GachaPool.Support;

internal sealed class ThrowingGachaRandomSource : IGachaRandomSource
{
    internal const string DiagnosticToken = "sensitive-diagnostic-token";

    public double NextUnitInterval()
    {
        throw new InvalidOperationException(DiagnosticToken);
    }
}
