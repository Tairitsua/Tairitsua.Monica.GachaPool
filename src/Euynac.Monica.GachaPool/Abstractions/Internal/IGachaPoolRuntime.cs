using Euynac.Monica.GachaPool.Models;

namespace Euynac.Monica.GachaPool.Abstractions.Internal;

internal interface IGachaPoolRuntime
{
    string Id { get; }

    Type PrizeType { get; }

    GachaDrawResult Draw(GachaDrawFilter filter);

    IReadOnlyList<GachaDrawResult> DrawMany(int count, GachaDrawFilter filter);

    GachaPoolSnapshot Snapshot();

    void Reset();
}
