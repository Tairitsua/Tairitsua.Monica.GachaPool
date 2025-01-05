using System.Threading.Tasks;

namespace CardPool.Interfaces;

/// <summary>
/// Defines the contract for loading card pools into a card pool manager.
/// </summary>
public interface ICardsPoolLoader
{
    /// <summary>
    /// Loads card pools into the specified manager.
    /// </summary>
    /// <param name="manager">The card pool manager to load pools into.</param>
    /// <returns>A task representing the asynchronous load operation.</returns>
    Task LoadPoolsAsync(ICardPoolManager manager);
} 