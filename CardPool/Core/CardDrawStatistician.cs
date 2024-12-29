using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace CardPool.Core;

public class CardDrawStatistician
{
    private readonly CardsPool _pool;
    private int _recordedTimes;
    public int RecordedTimes => _recordedTimes;
    public Dictionary<Card, StrongBox<int>> CardRecordDict { get; }
        
    /// <summary>
    /// Create a statistician to see the cards draw circumstance.
    /// </summary>
    /// <param name="pool"></param>
    public CardDrawStatistician(CardsPool pool)
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

    public string GetCurrentDescription()
    {
        var des = new StringBuilder();
        des.Append($"sum of all probability: {CardRecordDict.Keys.Sum(c => c.RealProbability)}\n");
        des.Append($"total drawn times: {_recordedTimes}\n");
        foreach (var (card, getTimes) in CardRecordDict)
        {
            var times = getTimes.Value;
            des.Append($"{card}".PadRight(25) + $"{times} - {times / (double) _recordedTimes:P4}".PadLeft(20));
            des.Append('\n');
        }
            
        return des.ToString().TrimEnd('\n');
    }
}