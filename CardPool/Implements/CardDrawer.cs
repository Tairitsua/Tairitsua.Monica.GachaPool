using System;
using System.Collections.Generic;
using System.Linq;
using CardPool.Conventions;
using CardPool.Interfaces;

namespace CardPool.Implements;

/// <summary>
/// Provides functionality for drawing cards from a card pool.
/// </summary>
public class CardDrawer : ICardDrawer
{
    /// <summary>
    /// Gets the card pool associated with this drawer.
    /// </summary>
    public ICardsPool Pool { get; }
    /// <summary>
    /// Gets the statistician responsible for tracking and analyzing card draw statistics.
    /// </summary>
    /// <remarks>
    /// The <see cref="CardDrawStatistician"/> provides functionality to record and retrieve
    /// statistical data related to card draws from the associated card pool.
    /// </remarks>
    public ICardDrawStatistician Statistician { get; }

    /// <summary>
    /// Initializes a new instance of the CardDrawer class.
    /// </summary>
    /// <param name="pool">The card pool to draw from.</param>
    public CardDrawer(ICardsPool pool)
    {
        Pool = pool;
        Statistician = new CardDrawStatistician(Pool);
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
        var availableCards = Pool.Cards.Where(card => card.Rarity == constrainedRarity && !card.IsRemoved).ToList();

        return InternalDrawCard(availableCards);
    }

    /// <summary>
    /// Draws a random card from the pool, limited to the specified included cards.
    /// </summary>
    /// <param name="includedCards">The cards to include in the draw.</param>
    /// <returns>The drawn card, or null if no cards from the included set are available.</returns>
    public Card DrawCardInclude(params Card[] includedCards)
    {
        var includedSet = new HashSet<Card>(includedCards);
        var availableCards = Pool.Cards.Where(card => includedSet.Contains(card) && !card.IsRemoved).ToList();
        return InternalDrawCard(availableCards);
    }




    /// <summary>
    /// Draws a random card from the pool, excluding specified cards.
    /// </summary>
    /// <param name="exclusiveCards">Cards to exclude from the draw.</param>
    /// <returns>The drawn card.</returns>
    public Card DrawCardExcept(params Card[] exclusiveCards)
    {
        var excludedSet = new HashSet<Card>(exclusiveCards);
        var availableCards = Pool.Cards.Where(card => !excludedSet.Contains(card) && !card.IsRemoved).ToList();
        return InternalDrawCard(availableCards);
    }


    /// <summary>
    /// Internal method for drawing a card from a specific range in the pool.
    /// </summary>
    /// <returns>The drawn card.</returns>
    protected Card InternalDrawCard(List<Card>? customCards = null)
    {
        var card = customCards != null
            ? Pool.InternalDrawCard(CardsPool.CreateBinarySearchLine(customCards))
            : Pool.InternalDrawCard();
        Statistician.RecordDrawnCard(card);
        return card;
    }
}

/// <summary>
/// Generic version of CardDrawer that provides strongly-typed card drawing functionality.
/// </summary>
/// <typeparam name="T">The type of card to draw.</typeparam>
public class CardDrawer<T> : CardDrawer, ICardDrawer<T> where T : notnull
{
    /// <summary>
    /// Initializes a new instance of the generic CardDrawer class.
    /// </summary>
    /// <param name="pool">The card pool to draw from.</param>
    public CardDrawer(ICardsPool pool) : base(pool)
    {

    }

    /// <summary>
    /// Draws a random card of type T from the pool.
    /// </summary>
    /// <returns>The drawn card.</returns>
    public new Card<T>? DrawCard()
    {
        return base.DrawCard() as Card<T>;
    }

    /// <summary>
    /// Draws a random card of type T with the specified rarity from the pool.
    /// </summary>
    /// <param name="constrainedRarity">The rarity of card to draw.</param>
    /// <returns>The drawn card, or null if no cards of the specified rarity exist.</returns>
    public new Card<T>? DrawCard(CardRarity constrainedRarity)
    {
        return base.DrawCard(constrainedRarity) as Card<T>;
    }

    /// <summary>
    /// Draws a random card of type T from the pool, including only the specified cards.
    /// </summary>
    /// <param name="includedCards">The cards to include in the draw.</param>
    /// <returns>The drawn card, or null if no cards from the specified list can be drawn.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when unable to draw a card from the inclusion list after maximum attempts.
    /// </exception>
    public Card<T>? DrawCardInclude(params Card<T>[] includedCards)
    {
        return DrawCardInclude(includedCards.Select(Card (p) => p).ToArray()) as Card<T>;
    }



    /// <summary>
    /// Draws a random card of type T from the pool, excluding specified cards.
    /// </summary>
    /// <param name="exclusiveCards">Cards to exclude from the draw.</param>
    /// <returns>The drawn card.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to draw a card that isn't in the exclusion list after maximum attempts.</exception>
    public Card<T>? DrawCardExcept(params Card<T>[] exclusiveCards)
    {
        return DrawCardExcept(exclusiveCards.Select(Card (p) => p).ToArray()) as Card<T>;
    }


    ///// <summary>
    ///// Internal method for drawing a card of type T from a specific range in the pool.
    ///// </summary>
    ///// <returns>The drawn card.</returns>
    //private new Card<T>? InternalDrawCard(List<Card>? customCards = null)
    //{
    //    return base.InternalDrawCard(customCards) as Card<T>;
    //}
}