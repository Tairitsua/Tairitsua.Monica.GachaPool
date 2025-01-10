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
        foreach (var card in _pool.Cards)
        {
            EnsureCardInDictionary(card);
        }
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

        return new CardDrawReport
        {
            TotalDraws = _recordedTimes,
            CardStats = cardStats,
            TotalProbability = _pool.Cards.Sum(c => c.RealProbability)
        };
    }
}