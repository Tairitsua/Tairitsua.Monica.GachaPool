using System.Collections.Generic;
using CardPool.Conventions;
using CardPool.Implements;

namespace CardPool.Interfaces;

/// <summary>
/// Defines the contract for a card pool that manages cards and their probabilities.
/// </summary>
public interface ICardsPool
{
    /// <summary>
    /// Gets the probability settings for different card rarities.
    /// </summary>
    Dictionary<CardRarity, double> RarityProbabilitySetting { get; }

    /// <summary>
    /// Gets all cards in the pool.
    /// </summary>
    IReadOnlyList<Card> Cards { get; }

    /// <summary>
    /// Gets or sets the remaining probability card.
    /// </summary>
    Card? RemainedCard { get; set; }

    /// <summary>
    /// Adds one or more cards to the pool.
    /// </summary>
    void AddCards(params Card?[] cards);

    /// <summary>
    /// Removes one or more cards from the pool.
    /// </summary>
    void RemoveCards(params Card?[] cards);

    /// <summary>
    /// Sets the probability for a specific card rarity in the pool.
    /// </summary>
    ICardsPool SetPoolRarityProbability(CardRarity rarity, double probability);

    /// <summary>
    /// Builds or rebuilds the card pool, calculating probabilities and preparing for drawing.
    /// </summary>
    void BuildPool();

    /// <summary>
    /// Gets a string representation of the pool's probability information.
    /// </summary>
    string GetPoolProbabilityInfo();

    /// <summary>
    /// Internal method for drawing a card from the pool.
    /// </summary>
    Card InternalDrawCard(BinarySearchLine? customSearchLine = null);
}