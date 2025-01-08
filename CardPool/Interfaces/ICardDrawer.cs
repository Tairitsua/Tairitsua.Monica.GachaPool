using System.Collections.Generic;
using CardPool.Conventions;

namespace CardPool.Interfaces;

/// <summary>
/// Defines the contract for a card drawer that handles drawing cards from a pool.
/// </summary>
public interface ICardDrawer
{
    /// <summary>
    /// Gets the card pool associated with this drawer.
    /// </summary>
    ICardsPool Pool { get; }

    /// <summary>
    /// Gets the statistician responsible for tracking and analyzing card draw statistics.
    /// </summary>
    ICardDrawStatistician Statistician { get; }

    /// <summary>
    /// Draws a random card from the pool.
    /// </summary>
    Card DrawCard();

    /// <summary>
    /// Draws a random card of the specified rarity from the pool.
    /// </summary>
    Card DrawCard(CardRarity constrainedRarity);

    /// <summary>
    /// Draws a random card from the pool, limited to the specified included cards.
    /// </summary>
    Card DrawCardInclude(params IEnumerable<Card> includedCards);

    /// <summary>
    /// Draws a random card from the pool, excluding specified cards.
    /// </summary>
    Card DrawCardExcept(params IEnumerable<Card> exclusiveCards);
}

/// <summary>
/// Defines the contract for a generic card drawer that handles drawing strongly-typed cards.
/// </summary>
public interface ICardDrawer<T> : ICardDrawer where T : notnull
{
    /// <summary>
    /// Draws a random card of type T from the pool.
    /// </summary>
    new Card<T>? DrawCard();

    /// <summary>
    /// Draws a random card of type T with the specified rarity from the pool.
    /// </summary>
    new Card<T>? DrawCard(CardRarity constrainedRarity);

    /// <summary>
    /// Draws a random card of type T from the pool, including only the specified cards.
    /// </summary>
    Card<T>? DrawCardInclude(params IEnumerable<Card<T>> includedCards);


    /// <summary>
    /// Draws a random card of type T from the pool, excluding the specified cards.
    /// </summary>
    /// <param name="exclusiveCards"></param>
    /// <returns></returns>
    Card<T>? DrawCardExcept(params IEnumerable<Card<T>> exclusiveCards);
}