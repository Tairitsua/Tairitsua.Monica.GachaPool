using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<Card, StrongBox<int>> _cardRecordDict = new();
    
    public int RecordedTimes => _recordedTimes;
    public Dictionary<Card, StrongBox<int>> CardRecordDict => _cardRecordDict.ToDictionary(x => x.Key, x => x.Value);

    /// <summary>
    /// Create a statistician to see the cards draw circumstance.
    /// </summary>
    /// <param name="pool"></param>
    public CardDrawStatistician(ICardsPool pool)
    {
        _pool = pool;
    }

    private void EnsureCardInDictionary(Card card)
    {
        _cardRecordDict.GetOrAdd(card, _ => new StrongBox<int>(0));
    }

    public void RecordDrawnCard(Card card)
    {
        EnsureCardInDictionary(card);
        Interlocked.Increment(ref _cardRecordDict[card].Value);
        Interlocked.Increment(ref _recordedTimes);
    }

    public CardDrawReport GetReport()
    {
        // Ensure all current pool cards are in the dictionary
        foreach (var card in _pool.Cards)
        {
            EnsureCardInDictionary(card);
        }
        if (_pool.RemainedCard != null)
        {
            EnsureCardInDictionary(_pool.RemainedCard);
        }

        var cardStats = _pool.Cards.Select(card =>
        {
            var drawCount = _cardRecordDict.GetOrAdd(card, _ => new StrongBox<int>(0)).Value;
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

        // Add remained card stats if it exists and isn't already in the pool
        if (_pool.RemainedCard != null && !_pool.Cards.Contains(_pool.RemainedCard))
        {
            var remainedCard = _pool.RemainedCard;
            var drawCount = _cardRecordDict.GetOrAdd(remainedCard, _ => new StrongBox<int>(0)).Value;
            cardStats.Add(new CardStatistics
            {
                Card = remainedCard,
                CardName = remainedCard.GetCardName(),
                ExpectedProbability = remainedCard.RealProbability,
                ActualProbability = _recordedTimes > 0 ? drawCount / (double)_recordedTimes : 0,
                DrawCount = drawCount,
                Rarity = remainedCard.Rarity
            });
        }

        return new CardDrawReport
        {
            TotalDraws = _recordedTimes,
            CardStats = cardStats,
            TotalProbability = _pool.Cards.Sum(c => c.RealProbability)
        };
    }
}