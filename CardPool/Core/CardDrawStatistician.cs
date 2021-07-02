using System.Collections.Generic;
using System.Text;

namespace CardPool.Core
{
    public class CardDrawStatistician
    {
        private CardsPool _pool;
        public int RecordedTimes { get; private set; }
        public Dictionary<Card, int> CardRecordDict { get; }
        
        /// <summary>
        /// Create a statistician to see the cards draw circumstance.
        /// </summary>
        /// <param name="pool"></param>
        public CardDrawStatistician(CardsPool pool)
        {
            _pool = pool;
            CardRecordDict = new Dictionary<Card, int>();
            foreach (var card in _pool.Cards)
            {
                CardRecordDict[card] = 0;
            }

            if (pool.RemainedCard != null && !CardRecordDict.ContainsKey(pool.RemainedCard))
            {
                CardRecordDict[pool.RemainedCard] = 0;
            }
        }

        public void RecordDrawnCard(Card card)
        {
            CardRecordDict[card]++;
            RecordedTimes++;
        }

        public string GetCurrentDescription()
        {
            var des = new StringBuilder();
            des.Append($"total drawn times: {RecordedTimes}\n");
            foreach (var (card, getTimes) in CardRecordDict)
            {
                des.Append($"{card}".PadRight(25) + $"{getTimes} - {getTimes / (double) RecordedTimes:P4}".PadLeft(20));
                des.Append('\n');
            }
            
            return des.ToString().TrimEnd('\n');
        }
    }
}