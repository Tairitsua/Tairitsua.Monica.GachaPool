using System;
using System.Linq;

namespace CardPool.Core
{
    public class CardDrawer
    {
        protected CardsPool Pool { get; }

        public CardDrawer(CardsPool pool)
        {
            Pool = pool;
        }

        public Card DrawCard()
        {
            return InternalDrawCard();
        }

        public Card DrawCard(CardRarity constrainedRarity)
        {
            if (!Pool.RarityInterval.ContainsKey(constrainedRarity)) return null;
            var (x, y) = Pool.RarityInterval[constrainedRarity];
            return InternalDrawCard(x, y);
        }
        
        public Card DrawCardExcept(params Card[] exclusiveCards)
        {
            var attemptTimes = 10000;
            while (attemptTimes-- != 0)
            {
                var drawn=  DrawCard();
                if (!exclusiveCards.Contains(drawn)) return drawn;
            }

            throw new InvalidOperationException("Attempt to draw card except failed, please reconsider the exclusive card list.");
        }
        
        
        protected Card InternalDrawCard(int? startIndex = null, int? endIndex = null)
        {
            return Pool.InternalDrawCard(startIndex, endIndex);
        }
    }
    
    public class CardDrawer<T> : CardDrawer where T : Card<T>
    {
        public CardDrawer(CardsPool pool) : base(pool)
        {
       
        }
        public new Card<T> DrawCard()
        {
            return InternalDrawCard();
        }

        public new Card<T> DrawCard(CardRarity constrainedRarity)
        {
            if (!Pool.RarityInterval.ContainsKey(constrainedRarity)) return null;
            var (x, y) = Pool.RarityInterval[constrainedRarity];
            return InternalDrawCard(x, y);
        }

        public Card<T> DrawCardExcept(params Card<T>[] exclusiveCards)
        {
            var attemptTimes = 10000;
            while (attemptTimes-- != 0)
            {
                var drawn=  DrawCard();
                if (!exclusiveCards.Contains(drawn)) return drawn;
            }

            throw new InvalidOperationException("Attempt to draw card except failed, please reconsider the exclusive card list.");
        }
        
        
        private new Card<T> InternalDrawCard(int? startIndex = null, int? endIndex = null)
        {
            var card = base.InternalDrawCard(startIndex, endIndex);
            if (card is Card<T> item) return item;
            return null;
        }

        
    }
}