using System;
using System.Collections.Generic;
using System.Threading;

namespace CardPool.Core;

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
    /// The card real probability relative to the entire card pool.
    /// </summary>
    public double RealProbability { get; internal set; }

    /// <summary>
    /// If not set, the real probability will auto generate according to rarity and the whole cards pool. The probability
    /// is relative to the entire card pool and not to the corresponding rarity cards. When set the probability, it also
    /// auto set the IsFixedRealProbability property true, means the real probability will be fixed forever.
    /// </summary>
    public double? SetProbability
    {
        get => _setProbability;
        set
        {
            if(value == null) return;
            _setProbability = value;
            RealProbability = _setProbability.Value;
            IsFixedRealProbability = true;
        }
    }

    /// <summary>
    /// Indicate that the card's real probability will not change even if the probability of some cards in pool changed. 
    /// </summary>
    public bool IsFixedRealProbability
    {
        get => (Attributes & CardAttributes.FixedRealProbability) != 0;
        set
        {
            if (value == false)
            {
                Attributes &= ~CardAttributes.FixedRealProbability;
            }
            else
            {
                Attributes |= CardAttributes.FixedRealProbability;
            }
        }
    }
    /// <summary>
    /// Indicate that the card has been removed and will not appear at card pool.
    /// (or to say the probability has becomes zero)
    /// </summary>
    public bool IsRemoved
    {
        get => (Attributes & CardAttributes.Removed) != 0;
        internal set
        {
            if (value == false)
            {
                Attributes &= ~CardAttributes.Removed;
            }
            else
            {
                Attributes |= CardAttributes.Removed;
                RealProbability = 0;
            }
        }
    }
        
    public CardAttributes Attributes { get; private set; }
        
    #region LimitedCard
    private int _totalCount;
    private int _remainCount;
    private double? _setProbability;

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
    public bool IsLimitedCard 
    {
        get => (Attributes & CardAttributes.Limited) != 0;
        private set
        {
            if (value == false)
            {
                Attributes &= ~CardAttributes.Limited;
            }
            else
            {
                Attributes |= CardAttributes.Limited;
            }
        }
    }
    /// <summary>
    /// Try to subtract the number of cards to ensure that the card was successfully obtained.
    /// Only available when the card is limited card.
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
        return $"{RealProbability:P5} [{Rarity}]";
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
    public NothingCard(double remainRealProbability)
    {
        RealProbability = remainRealProbability;
    }

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
/// <summary>
/// Represents a generic card with a strongly-typed value.
/// </summary>
/// <typeparam name="T">The type of the card's value.</typeparam>
public class Card<T> : Card
{
    /// <summary>
    /// Gets or sets the value associated with this card.
    /// </summary>
    public T Item { get; set; }

    /// <summary>
    /// Initializes a new instance of the Card{T} class.
    /// </summary>
    /// <param name="item">The value to associate with this card.</param>
    public Card(T item)
    {
        Item = item;
    }

    /// <summary>
    /// Creates multiple cards of the same rarity with different values.
    /// </summary>
    /// <param name="rarity">The rarity to assign to all created cards.</param>
    /// <param name="cards">The values to create cards from.</param>
    /// <returns>A list of created cards.</returns>
    public static List<Card<T>> CreateMultiCards(CardRarity rarity, params T[] cards)
    {
        var createdCards = new List<Card<T>>();
        foreach (var card in cards)
        {
            var newCard = new Card<T>(card)
            {
                Rarity = rarity
            };
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

/// <summary>
/// Extension methods for card collections.
/// </summary>
public static class CardExtension
{
    /// <summary>
    /// Performs an action on each card in a collection.
    /// </summary>
    /// <param name="cards">The collection of cards to process.</param>
    /// <param name="action">The action to perform on each card.</param>
    /// <returns>The original collection of cards.</returns>
    public static ICollection<Card> EachCardSet(this ICollection<Card> cards, Action<Card> action)
    {
        foreach (var card in cards)
        {
            action(card);
        }
        return cards;
    }
    /// <summary>
    /// Performs an action on each strongly-typed card in a collection.
    /// </summary>
    /// <typeparam name="T">The type of the cards' values.</typeparam>
    /// <param name="cards">The collection of cards to process.</param>
    /// <param name="action">The action to perform on each card.</param>
    /// <returns>The original collection of cards.</returns>
    public static ICollection<Card<T>> EachCardSet<T>(this ICollection<Card<T>> cards, Action<Card<T>> action)
    {
        foreach (var card in cards)
        {
            action(card);
        }
        return cards;
    }
}