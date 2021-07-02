using System;
using System.Collections.Generic;
using System.Threading;

namespace CardPool.Core
{
    public abstract class Card
    {
        

        /// <summary>
        /// The card rarity. The global probability will auto generate base on this.
        /// </summary>
        public CardRarity Rarity { get; set; }
        /// <summary>
        /// The probability is relative to all the cards which have the same rarity with the card. Only available when
        /// the Probability property is null.
        /// </summary>
        public double? ProbabilityOfSameRarityPool { get; set; }
        /// <summary>
        /// If not set, the probability will auto generate according to rarity and the whole cards pool. The probability
        /// is relative to the entire card pool and not to the corresponding rarity cards.
        /// </summary>
        public double? Probability { get; set; }


        #region LimitedCard
        private int _totalCount;
        private int _remainCount;
        /// <summary>
        /// If set total count, it means the card is limited, and its probability will become zero when this
        /// kind of cards run out. Default is null which means is infinite.
        /// </summary>
        public int TotalCount
        {
            get => _totalCount;
            set
            {
                _totalCount = value;
                _remainCount = value;
                IsLimitedCard = value > 0;
            }
        }
        /// <summary>
        /// The limited card remaining count.
        /// </summary>
        public int RemainCount => _remainCount;
        /// <summary>
        /// This property will auto turn to true when you set the card TotalCount property.
        /// </summary>
        public bool IsLimitedCard { get; private set; }
        /// <summary>
        /// Try to subtract the number of cards to ensure that the card was successfully obtained. Only available when the card is limited card.
        /// </summary>
        /// <returns></returns>
        internal bool SuccessGetCard()
        {
            if (!IsLimitedCard) return true;
            if (_remainCount <= 0) return false;
            return Interlocked.Decrement(ref _remainCount) >= 0;
        }

        #endregion
        
        
        
        
        public override string ToString()
        {
            return $"{Probability:P5} [{Rarity}]";
        }

        /// <summary>
        /// The rarity of the card.
        /// </summary>
        public enum CardRarity
        {
            ZeroStar,
            OneStar,
            TwoStar,
            ThreeStar,
            FourStar,
            FiveStar,
            SixStar,
            SevenStar,
            EightStar,
            NineStar,
            TenStar
        }
    }
    
    /// <summary>
    /// When a cards pool placed all valid card, the rest probability will turn to a nothing card.
    /// </summary>
    public class NothingCard : Card
    {
        public NothingCard()
        {
            
        }
        public NothingCard(double remainProbability)
        {
            Probability = remainProbability;
        }

        // public override bool Equals(object? obj)
        // {
        //     return obj is NothingCard nothingCard && (Probability == null && nothingCard.Probability == null ||
        //                                               nothingCard.Probability != null &&
        //                                               Probability != null &&
        //                                               Math.Abs(nothingCard.Probability.Value - Probability.Value) < 1E-7);
        // }
        //
        // public override int GetHashCode()
        // {
        //     return (int)(Probability ?? 1) * 13131313;
        // }

        public override bool Equals(object? obj)
        {
            return obj is NothingCard;
        }

        public override int GetHashCode()
        {
            return 13131313;//Always equal when the card is nothing card.
        }
        public override string ToString()
        {
            return $"Nothing ———— {base.ToString()}";
        }
    }
    public class Card<T> : Card
    {
        public T Item { get; set; }

        public Card(T item)
        {
            Item = item;
        }

        public static List<Card<T>> CreateMultiCards(CardRarity rarity, params T[] cards)
        {
            List<Card<T>> createdCards = new List<Card<T>>();
            foreach (var card in cards)
            {
                var newCard = new Card<T>(card);
                newCard.Rarity = rarity;
                createdCards.Add(newCard);
            }

            return createdCards;
        }

        public override bool Equals(object? obj)
        {
            if (obj is Card<T> card)
            {
                return EqualityComparer<T>.Default.Equals(card.Item, Item);
            }

            return false;
        }
        
        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Item);
        }

        public override string ToString()
        {
            return $"{Item.ToString()} ———— {base.ToString()}";
        }
    }


    public static class CardExtension
    {
        public static ICollection<Card> EachCardSet(this ICollection<Card> cards, Action<Card> action)
        {
            foreach (var card in cards)
            {
                action(card);
            }
            return cards;
        }
        public static ICollection<Card<T>> EachCardSet<T>(this ICollection<Card<T>> cards, Action<Card<T>> action)
        {
            foreach (var card in cards)
            {
                action(card);
            }
            return cards;
        }
    }
}