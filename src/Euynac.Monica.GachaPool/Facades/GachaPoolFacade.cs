using Euynac.Monica.GachaPool.Abstractions;
using Euynac.Monica.GachaPool.Localization;
using Euynac.Monica.GachaPool.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Monica.Core.Results;

namespace Euynac.Monica.GachaPool.Facades;

/// <summary>
/// Provides host-facing gacha pool queries, draws, and reset operations for API and UI consumers.
/// </summary>
public sealed partial class GachaPoolFacade(
    IGachaPoolCatalog catalog,
    IStringLocalizer<GachaPoolResource> localizer,
    ILogger<GachaPoolFacade> logger)
{
    /// <summary>
    /// Gets snapshots of every pool registered in the current Monica host.
    /// </summary>
    /// <returns>A successful result with ordered pool snapshots, or an internal-error result.</returns>
    public Res<IReadOnlyList<GachaPoolSnapshot>> GetPools()
    {
        try
        {
            return Res.Ok(catalog.GetPools());
        }
        catch (Exception exception)
        {
            return InternalFailure<IReadOnlyList<GachaPoolSnapshot>>(
                exception,
                localizer["ServiceMessages:ListFailed"]);
        }
    }

    /// <summary>
    /// Gets one pool snapshot.
    /// </summary>
    /// <param name="poolId">The pool identifier.</param>
    /// <returns>A successful result, a request error, a not-found result, or an internal-error result.</returns>
    public Res<GachaPoolSnapshot> GetPool(string poolId)
    {
        if (string.IsNullOrWhiteSpace(poolId))
        {
            return InvalidPoolId<GachaPoolSnapshot>();
        }

        try
        {
            return Res.Ok(catalog.GetPool(poolId));
        }
        catch (KeyNotFoundException)
        {
            return Res.Fail(
                ResStatus.NotFound,
                "{0}",
                localizer["ServiceMessages:PoolNotFound", poolId]);
        }
        catch (Exception exception)
        {
            return InternalFailure<GachaPoolSnapshot>(
                exception,
                localizer["ServiceMessages:GetFailed", poolId]);
        }
    }

    /// <summary>
    /// Performs one presentation-safe draw.
    /// </summary>
    /// <param name="poolId">The pool identifier.</param>
    /// <param name="filter">Optional draw restriction.</param>
    /// <returns>A successful draw result, a request error, a not-found result, or an internal-error result.</returns>
    public Res<GachaDrawResult> Draw(string poolId, GachaDrawFilter? filter = null)
    {
        if (string.IsNullOrWhiteSpace(poolId))
        {
            return InvalidPoolId<GachaDrawResult>();
        }

        try
        {
            return Res.Ok(catalog.Draw(poolId, filter));
        }
        catch (KeyNotFoundException)
        {
            return Res.Fail(
                ResStatus.NotFound,
                "{0}",
                localizer["ServiceMessages:PoolNotFound", poolId]);
        }
        catch (ArgumentException)
        {
            return Res.Fail(
                ResStatus.BadRequest,
                "{0}",
                localizer["ServiceMessages:DrawInvalid"]);
        }
        catch (Exception exception)
        {
            return InternalFailure<GachaDrawResult>(
                exception,
                localizer["ServiceMessages:DrawFailed", poolId]);
        }
    }

    /// <summary>
    /// Performs a bounded batch of presentation-safe draws.
    /// </summary>
    /// <param name="poolId">The pool identifier.</param>
    /// <param name="count">The number of draws.</param>
    /// <param name="filter">Optional draw restriction applied to every draw.</param>
    /// <returns>A successful draw list, a request error, a not-found result, or an internal-error result.</returns>
    public Res<IReadOnlyList<GachaDrawResult>> DrawMany(
        string poolId,
        int count,
        GachaDrawFilter? filter = null)
    {
        if (string.IsNullOrWhiteSpace(poolId))
        {
            return InvalidPoolId<IReadOnlyList<GachaDrawResult>>();
        }

        try
        {
            return Res.Ok(catalog.DrawMany(poolId, count, filter));
        }
        catch (KeyNotFoundException)
        {
            return Res.Fail(
                ResStatus.NotFound,
                "{0}",
                localizer["ServiceMessages:PoolNotFound", poolId]);
        }
        catch (ArgumentException)
        {
            return Res.Fail(
                ResStatus.BadRequest,
                "{0}",
                localizer["ServiceMessages:DrawInvalid"]);
        }
        catch (Exception exception)
        {
            return InternalFailure<IReadOnlyList<GachaDrawResult>>(
                exception,
                localizer["ServiceMessages:DrawFailed", poolId]);
        }
    }

    /// <summary>
    /// Restores limited inventory and clears statistics for one pool.
    /// </summary>
    /// <param name="poolId">The pool identifier.</param>
    /// <returns>A success, request error, not-found result, or internal-error result.</returns>
    public Res Reset(string poolId)
    {
        if (string.IsNullOrWhiteSpace(poolId))
        {
            return InvalidPoolId();
        }

        try
        {
            catalog.Reset(poolId);
            return Res.Ok(localizer["ServiceMessages:ResetCompleted", poolId]);
        }
        catch (KeyNotFoundException)
        {
            return Res.Fail(
                ResStatus.NotFound,
                "{0}",
                localizer["ServiceMessages:PoolNotFound", poolId]);
        }
        catch (Exception exception)
        {
            LogResetFailure(logger, exception, poolId);
            return Res.Fail(
                ResStatus.InternalError,
                "{0}",
                localizer["ServiceMessages:ResetFailed", poolId]);
        }
    }

    private Res<T> InternalFailure<T>(Exception exception, string message)
    {
        LogInternalFailure(logger, exception, message);
        return Res.Fail(
            ResStatus.InternalError,
            "{0}",
            message);
    }

    private Res<T> InvalidPoolId<T>()
    {
        return InvalidPoolId();
    }

    private Res InvalidPoolId()
    {
        return Res.Fail(
            ResStatus.BadRequest,
            "{0}",
            localizer["ServiceMessages:PoolIdInvalid"]);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Failed to reset gacha pool {PoolId}.")]
    private static partial void LogResetFailure(ILogger logger, Exception exception, string poolId);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Error,
        Message = "{FailureMessage}")]
    private static partial void LogInternalFailure(ILogger logger, Exception exception, string failureMessage);
}
