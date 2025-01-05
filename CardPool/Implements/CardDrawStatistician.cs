using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

    public string GetReportTableString()
    {
        var des = new StringBuilder();
        des.AppendLine($"sum of all probability: {CardRecordDict.Keys.Sum(c => c.RealProbability)}");
        des.AppendLine($"total drawn times: {_recordedTimes}");
        des.AppendLine(new string('-', 80));
        des.AppendLine($"{"CardName",-20}{"ExpectProb",-15}{"Rarity",-10}{"DrawnCount",-15}{"ExactProb",-15}");
        des.AppendLine(new string('-', 80));
        foreach (var (card, getTimes) in CardRecordDict)
        {
            var times = getTimes.Value;
            des.AppendLine($"{card.GetCardName().PadRight(20)}" +
                           $"{card.RealProbability.ToString("P4").PadRight(15)}" +
                           $"{card.Rarity.ToString().PadRight(10)}" +
                           $"{times.ToString().PadRight(15)}" +
                           $"{(times / (double)_recordedTimes).ToString("P4").PadRight(15)}");
        }
        des.AppendLine(new string('-', 80));
        return des.ToString().TrimEnd('\n');
    }

}