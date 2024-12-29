using System;
using System.Linq;

namespace CardPool.Core;

/// <summary>
/// Provides functionality for drawing cards from a card pool.
/// </summary>
public class CardDrawer
{
    /// <summary>
    /// Gets the card pool associated with this drawer.
    /// </summary>
    protected CardsPool Pool { get; }

    /// <summary>
    /// Initializes a new instance of the CardDrawer class.
    /// </summary>
    /// <param name="pool">The card pool to draw from.</param>
    public CardDrawer(CardsPool pool)
    {
        Pool = pool;
    }

    /// <summary>
    /// Draws a random card from the pool.
    /// </summary>
    /// <returns>The drawn card.</returns>
    public Card DrawCard()
    {
        return InternalDrawCard();
    }

    /// <summary>
    /// Draws a random card of the specified rarity from the pool.
    /// </summary>
    /// <param name="constrainedRarity">The rarity of card to draw.</param>
    /// <returns>The drawn card, or null if no cards of the specified rarity exist.</returns>
    public Card DrawCard(CardRarity constrainedRarity)
    {
        if (!Pool.RarityInterval.TryGetValue(constrainedRarity, out var value)) return new NothingCard();
        var (x, y) = value;
        return InternalDrawCard(x, y);
    }
        
    /// <summary>
    /// Draws a random card from the pool, excluding specified cards.
    /// </summary>
    /// <param name="exclusiveCards">Cards to exclude from the draw.</param>
    /// <returns>The drawn card.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to draw a card that isn't in the exclusion list after maximum attempts.</exception>
    public Card DrawCardExcept(params Card[] exclusiveCards)
    {
        //TODO should test whether it really can draw a card from the pool.
        var attemptTimes = 10000;
        while (attemptTimes-- != 0)
        {
            var drawn=  DrawCard();
            if (!exclusiveCards.Contains(drawn)) return drawn;
        }

        throw new InvalidOperationException("Attempt to draw card except failed, please reconsider the exclusive card list.");
    }
        
        
    /// <summary>
    /// Internal method for drawing a card from a specific range in the pool.
    /// </summary>
    /// <param name="startIndex">Optional starting index in the pool.</param>
    /// <param name="endIndex">Optional ending index in the pool.</param>
    /// <returns>The drawn card.</returns>
    protected Card InternalDrawCard(int? startIndex = null, int? endIndex = null)
    {
        return Pool.InternalDrawCard(startIndex, endIndex);
    }
}
    
/// <summary>
/// Generic version of CardDrawer that provides strongly-typed card drawing functionality.
/// </summary>
/// <typeparam name="T">The type of card to draw.</typeparam>
public class CardDrawer<T> : CardDrawer where T : Card<T>
{
    /// <summary>
    /// Initializes a new instance of the generic CardDrawer class.
    /// </summary>
    /// <param name="pool">The card pool to draw from.</param>
    public CardDrawer(CardsPool pool) : base(pool)
    {
       
    }

    /// <summary>
    /// Draws a random card of type T from the pool.
    /// </summary>
    /// <returns>The drawn card.</returns>
    public new Card<T> DrawCard()
    {
        return InternalDrawCard();
    }

    /// <summary>
    /// Draws a random card of type T with the specified rarity from the pool.
    /// </summary>
    /// <param name="constrainedRarity">The rarity of card to draw.</param>
    /// <returns>The drawn card, or null if no cards of the specified rarity exist.</returns>
    public new Card<T>? DrawCard(CardRarity constrainedRarity)
    {
        if (!Pool.RarityInterval.TryGetValue(constrainedRarity, out var value)) return null;
        var (x, y) = value;
        return InternalDrawCard(x, y);
    }

    /// <summary>
    /// Draws a random card of type T from the pool, excluding specified cards.
    /// </summary>
    /// <param name="exclusiveCards">Cards to exclude from the draw.</param>
    /// <returns>The drawn card.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to draw a card that isn't in the exclusion list after maximum attempts.</exception>
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
        
        
    /// <summary>
    /// Internal method for drawing a card of type T from a specific range in the pool.
    /// </summary>
    /// <param name="startIndex">Optional starting index in the pool.</param>
    /// <param name="endIndex">Optional ending index in the pool.</param>
    /// <returns>The drawn card.</returns>
    private new Card<T>? InternalDrawCard(int? startIndex = null, int? endIndex = null)
    {
        var card = base.InternalDrawCard(startIndex, endIndex);
        if (card is Card<T> item) return item;
        return null;
    }

        
}