using Tairitsua.Monica.GachaPool.Models;

namespace Tairitsua.Monica.GachaPool.Abstractions.Internal;

internal interface IGachaPoolRuntime
{
    string Id { get; }

    Type PrizeType { get; }

    GachaDrawResult Draw(GachaDrawFilter filter);

    IReadOnlyList<GachaDrawResult> DrawMany(int count, GachaDrawFilter filter);

    GachaPoolSnapshot Snapshot();

    void Reset();
}
