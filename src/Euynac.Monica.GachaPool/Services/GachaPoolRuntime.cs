using Euynac.Monica.GachaPool.Abstractions;
using Euynac.Monica.GachaPool.Abstractions.Internal;
using Euynac.Monica.GachaPool.Models;
using Euynac.Monica.GachaPool.Models.Internal;

namespace Euynac.Monica.GachaPool.Services;

internal sealed class GachaPoolRuntime<TPrize>(
    GachaPoolDefinition<TPrize> definition,
    int recentDrawHistoryLimit,
    IGachaRandomSource randomSource) : IGachaPoolRuntime where TPrize : notnull
{
    private readonly object _syncRoot = new();
    private readonly GachaPrizeRuntime<TPrize>[] _prizes = definition.Prizes
        .Select(static prize => new GachaPrizeRuntime<TPrize>(prize))
        .ToArray();
    private readonly Queue<GachaDrawResult> _recentDraws = [];
    private long _sequence;
    private long _totalDraws;
    private long _noPrizeDraws;

    public string Id => definition.Id;

    public Type PrizeType => typeof(TPrize);

    public GachaDrawResult Draw(GachaDrawFilter filter)
    {
        return DrawTyped(filter).ToPresentationResult();
    }

    public IReadOnlyList<GachaDrawResult> DrawMany(int count, GachaDrawFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        lock (_syncRoot)
        {
            var draws = new GachaDrawResult[count];
            for (var index = 0; index < count; index++)
            {
                draws[index] = DrawCore(filter).ToPresentationResult();
            }

            return draws;
        }
    }

    internal GachaDrawResult<TPrize> DrawTyped(GachaDrawFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        lock (_syncRoot)
        {
            return DrawCore(filter);
        }
    }

    public GachaPoolSnapshot Snapshot()
    {
        lock (_syncRoot)
        {
            var availableWeightByRarity = _prizes
                .Where(static prize => prize.IsAvailable)
                .GroupBy(static prize => prize.Definition.Rarity)
                .ToDictionary(
                    static group => group.Key,
                    static group => group.Sum(static prize => prize.Definition.Weight));

            var entries = _prizes.Select(prize =>
            {
                var definitionProbability = definition.RarityProbabilities[prize.Definition.Rarity];
                var probability = prize.IsAvailable
                                  && availableWeightByRarity.TryGetValue(prize.Definition.Rarity, out var availableWeight)
                    ? definitionProbability * prize.Definition.Weight / availableWeight
                    : 0;
                var observedProbability = _totalDraws == 0 ? 0 : (double)prize.DrawCount / _totalDraws;

                return new GachaEntrySnapshot(
                    ToSnapshot(prize.Definition),
                    prize.Definition.Weight,
                    probability,
                    prize.Definition.InitialStock,
                    prize.RemainingStock,
                    prize.DrawCount,
                    observedProbability);
            }).ToArray();

            var depletedProbability = definition.RarityProbabilities
                .Where(pair => !availableWeightByRarity.ContainsKey(pair.Key))
                .Sum(static pair => pair.Value);

            return new GachaPoolSnapshot(
                definition.Id,
                definition.DisplayName,
                definition.Description,
                typeof(TPrize).FullName ?? typeof(TPrize).Name,
                Math.Min(1, definition.NoPrizeProbability + depletedProbability),
                _totalDraws,
                _noPrizeDraws,
                entries,
                _recentDraws.Reverse().ToArray());
        }
    }

    public void Reset()
    {
        lock (_syncRoot)
        {
            foreach (var prize in _prizes)
            {
                prize.Reset();
            }

            _sequence = 0;
            _totalDraws = 0;
            _noPrizeDraws = 0;
            _recentDraws.Clear();
        }
    }

    private GachaDrawResult<TPrize> DrawCore(GachaDrawFilter filter)
    {
        return filter.IsUnrestricted
            ? DrawUnrestricted()
            : DrawRestricted(filter);
    }

    private GachaDrawResult<TPrize> DrawUnrestricted()
    {
        var roll = NextRoll();
        var probabilityCursor = 0d;

        foreach (var rarityProbability in definition.RarityProbabilities.OrderBy(static pair => pair.Key))
        {
            var nextCursor = probabilityCursor + rarityProbability.Value;
            if (roll >= probabilityCursor && roll < nextCursor)
            {
                var availablePrizes = _prizes
                    .Where(prize => prize.IsAvailable && prize.Definition.Rarity == rarityProbability.Key)
                    .ToArray();
                if (availablePrizes.Length == 0)
                {
                    return CompleteNonPrizeDraw(GachaDrawOutcomeKind.Exhausted, rarityProbability.Value);
                }

                var localRoll = (roll - probabilityCursor) / rarityProbability.Value;
                var selected = SelectByWeight(availablePrizes, localRoll, out var withinRarityProbability);
                return CompletePrizeDraw(selected, rarityProbability.Value * withinRarityProbability);
            }

            probabilityCursor = nextCursor;
        }

        return CompleteNonPrizeDraw(GachaDrawOutcomeKind.NoPrize, Math.Max(0, 1 - probabilityCursor));
    }

    private GachaDrawResult<TPrize> DrawRestricted(GachaDrawFilter filter)
    {
        var eligiblePrizes = _prizes
            .Where(prize => prize.IsAvailable && filter.Matches(prize.Definition))
            .ToArray();
        if (eligiblePrizes.Length == 0)
        {
            return CompleteNonPrizeDraw(GachaDrawOutcomeKind.Exhausted, 1);
        }

        var weightedPrizes = eligiblePrizes
            .GroupBy(static prize => prize.Definition.Rarity)
            .SelectMany(group =>
            {
                var rarityProbability = definition.RarityProbabilities[group.Key];
                var totalWeight = group.Sum(static prize => prize.Definition.Weight);
                return group.Select(prize => (
                    Prize: prize,
                    Score: rarityProbability * prize.Definition.Weight / totalWeight));
            })
            .ToArray();
        var totalScore = weightedPrizes.Sum(static item => item.Score);
        var roll = NextRoll() * totalScore;
        var cursor = 0d;

        foreach (var item in weightedPrizes)
        {
            cursor += item.Score;
            if (roll < cursor)
            {
                return CompletePrizeDraw(item.Prize, item.Score / totalScore);
            }
        }

        var fallback = weightedPrizes[^1];
        return CompletePrizeDraw(fallback.Prize, fallback.Score / totalScore);
    }

    private GachaDrawResult<TPrize> CompletePrizeDraw(
        GachaPrizeRuntime<TPrize> prize,
        double probabilityAtDraw)
    {
        prize.RecordDraw();
        return CompleteDraw(
            GachaDrawOutcomeKind.Prize,
            prize.Definition,
            probabilityAtDraw,
            prize.RemainingStock);
    }

    private GachaDrawResult<TPrize> CompleteNonPrizeDraw(
        GachaDrawOutcomeKind outcome,
        double probabilityAtDraw)
    {
        return CompleteDraw(outcome, null, probabilityAtDraw, null);
    }

    private GachaDrawResult<TPrize> CompleteDraw(
        GachaDrawOutcomeKind outcome,
        GachaPrizeDefinition<TPrize>? prize,
        double probabilityAtDraw,
        int? remainingStock)
    {
        _sequence++;
        _totalDraws++;
        if (outcome != GachaDrawOutcomeKind.Prize)
        {
            _noPrizeDraws++;
        }

        var result = new GachaDrawResult<TPrize>(
            definition.Id,
            _sequence,
            DateTimeOffset.UtcNow,
            outcome,
            prize is null ? null : ToSnapshot(prize),
            prize is null ? default! : prize.Value,
            probabilityAtDraw,
            remainingStock);
        if (recentDrawHistoryLimit > 0)
        {
            _recentDraws.Enqueue(result.ToPresentationResult());
            while (_recentDraws.Count > recentDrawHistoryLimit)
            {
                _recentDraws.Dequeue();
            }
        }

        return result;
    }

    private static GachaPrizeRuntime<TPrize> SelectByWeight(
        IReadOnlyList<GachaPrizeRuntime<TPrize>> prizes,
        double roll,
        out double probability)
    {
        var totalWeight = prizes.Sum(static prize => prize.Definition.Weight);
        var target = roll * totalWeight;
        var cursor = 0d;

        foreach (var prize in prizes)
        {
            cursor += prize.Definition.Weight;
            if (target < cursor)
            {
                probability = prize.Definition.Weight / totalWeight;
                return prize;
            }
        }

        var fallback = prizes[^1];
        probability = fallback.Definition.Weight / totalWeight;
        return fallback;
    }

    private static GachaPrizeSnapshot ToSnapshot(GachaPrizeDefinition<TPrize> prize)
    {
        return new GachaPrizeSnapshot(prize.Id, prize.DisplayName, prize.Description, prize.Rarity);
    }

    private double NextRoll()
    {
        var roll = randomSource.NextUnitInterval();
        if (!double.IsFinite(roll) || roll < 0 || roll >= 1)
        {
            throw new InvalidOperationException(
                $"{nameof(IGachaRandomSource)} returned {roll}; values must be finite and belong to [0, 1).");
        }

        return roll;
    }
}
