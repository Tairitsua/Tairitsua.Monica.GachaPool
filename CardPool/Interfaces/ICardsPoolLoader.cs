using System.Threading.Tasks;

namespace CardPool.Interfaces;

/// <summary>
/// Defines the contract for loading and configuring card pools.
/// </summary>
public interface ICardsPoolLoader
{
    /// <summary>
    /// Configures the card pools that will be loaded.
    /// </summary>
    void ConfigurePools();

    /// <summary>
    /// Loads the configured card pools into the specified manager.
    /// </summary>
    /// <param name="manager">The card pool manager to load pools into.</param>
    /// <returns>A task representing the asynchronous load operation.</returns>
    Task LoadPoolsAsync(ICardPoolManager manager);
} 