using System.Collections.Generic;
using System.Text;

namespace MoLibrary.GachaPool.Conventions;

/// <summary>
/// Represents a detailed report of card draw statistics.
/// </summary>
public class CardDrawReport
{
    /// <summary>
    /// Gets the total number of times cards have been drawn.
    /// </summary>
    public int TotalDraws { get; init; }

    /// <summary>
    /// Gets the collection of individual card statistics.
    /// </summary>
    public IReadOnlyList<CardStatistics> CardStats { get; init; } = [];

    /// <summary>
    /// Gets the total probability sum of all cards.
    /// </summary>
    public double TotalProbability { get; init; }

    /// <summary>
    /// Gets a formatted string representation of the draw statistics.
    /// </summary>
    /// <returns>A formatted string containing the draw statistics in a table format.</returns>
    public string GetTableString()
    {
        var des = new StringBuilder();
        des.AppendLine($"sum of all probability: {TotalProbability}");
        des.AppendLine($"total drawn times: {TotalDraws}");
        des.AppendLine(new string('-', 80));
        des.AppendLine($"{"CardName",-20}{"ExpectProb",-15}{"Rarity",-10}{"DrawnCount",-15}{"ExactProb",-15}");
        des.AppendLine(new string('-', 80));
        
        foreach (var stat in CardStats)
        {
            des.AppendLine($"{stat.CardName.PadRight(20)}" +
                          $"{stat.ExpectedProbability.ToString("P4").PadRight(15)}" +
                          $"{stat.Rarity.ToString().PadRight(10)}" +
                          $"{stat.DrawCount.ToString().PadRight(15)}" +
                          $"{stat.ActualProbability.ToString("P4").PadRight(15)}");
        }
        
        des.AppendLine(new string('-', 80));
        return des.ToString().TrimEnd('\n');
    }
}

/// <summary>
/// Represents statistics for an individual card.
/// </summary>
public class CardStatistics
{
    /// <summary>
    /// Gets the card associated with these statistics.
    /// </summary>
    public Card Card { get; init; } = null!;

    /// <summary>
    /// Gets the name of the card.
    /// </summary>
    public string CardName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the expected probability of drawing this card.
    /// </summary>
    public double ExpectedProbability { get; init; }

    /// <summary>
    /// Gets the actual probability based on draw history.
    /// </summary>
    public double ActualProbability { get; init; }

    /// <summary>
    /// Gets the number of times this card has been drawn.
    /// </summary>
    public int DrawCount { get; init; }

    /// <summary>
    /// Gets the rarity of the card.
    /// </summary>
    public CardRarity Rarity { get; init; }
} 