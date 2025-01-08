using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using CardPool.Conventions;
using CardPool.Interfaces;

namespace CardPool.Implements;

public class CardDrawStatistician : ICardDrawStatistician
{
    private readonly ICardsPool _pool;
    private int _recordedTimes;
    public int RecordedTimes => _recordedTimes;
    public Dictionary<Card, StrongBox<int>> CardRecordDict { get; }

    /// <summary>
    /// Create a statistician to see the cards draw circumstance.
    /// </summary>
    /// <param name="pool"></param>
    public CardDrawStatistician(ICardsPool pool)
    {
        _pool = pool;
        CardRecordDict = new Dictionary<Card, StrongBox<int>>();
        foreach (var card in _pool.Cards)
        {
            CardRecordDict[card] = new StrongBox<int>(0);
        }

        if (pool.RemainedCard != null && !CardRecordDict.ContainsKey(pool.RemainedCard))
        {
            CardRecordDict[pool.RemainedCard] = new StrongBox<int>(0);
        }
    }

    public void RecordDrawnCard(Card card)
    {
        Interlocked.Increment(ref CardRecordDict[card].Value);
        Interlocked.Increment(ref _recordedTimes);
    }

    public CardDrawReport GetReport()
    {
        var cardStats = CardRecordDict.Select(kvp =>
        {
            var card = kvp.Key;
            var drawCount = kvp.Value.Value;
            return new CardStatistics
            {
                Card = card,
                CardName = card.GetCardName(),
                ExpectedProbability = card.RealProbability,
                ActualProbability = _recordedTimes > 0 ? drawCount / (double)_recordedTimes : 0,
                DrawCount = drawCount,
                Rarity = card.Rarity
            };
        }).ToList();

        return new CardDrawReport
        {
            TotalDraws = _recordedTimes,
            CardStats = cardStats,
            TotalProbability = CardRecordDict.Keys.Sum(c => c.RealProbability)
        };
    }
}