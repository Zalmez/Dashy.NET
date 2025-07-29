using System.Collections.Concurrent;
using System.Security.Claims;
using Dashy.Net.Shared.Models;
namespace Dashy.Net.Web.Services;

/// <summary>
/// Global edit lock service that manages edit locks across all clients.
/// This ensures only one user can edit a dashboard configuration at a time.
/// </summary>
public class EditLockService
{
    private readonly ConcurrentDictionary<int, EditLockInfo> _dashboardLocks = new();
    private readonly ILogger<EditLockService> _logger;
    private readonly Timer _cleanupTimer;
    private static readonly TimeSpan LockTimeout = TimeSpan.FromMinutes(5);

    public EditLockService(ILogger<EditLockService> logger)
    {
        _logger = logger;
        _cleanupTimer = new Timer(CleanupExpiredLocks, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Event fired when a lock is acquired, released, or expires.
    /// This notifies all ViewOptionsService instances across all clients.
    /// </summary>
    public event Action<int, EditLockInfo?>? OnLockChanged;

    /// <summary>
    /// Gets the current lock for a dashboard (if any).
    /// </summary>
    public EditLockInfo? GetCurrentLock(int dashboardId)
    {
        _dashboardLocks.TryGetValue(dashboardId, out var lockInfo);
        return lockInfo;
    }

    /// <summary>
    /// Attempts to acquire an edit lock for a dashboard.
    /// Returns true if successful, false if another user holds the lock.
    /// </summary>
    public bool TryAcquireLock(int dashboardId, string userId, string userName, string connectionId)
    {
        var newLock = new EditLockInfo
        {
            UserId = userId,
            UserName = userName,
            LockedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow,
            ConnectionId = connectionId
        };

        var acquired = _dashboardLocks.AddOrUpdate(
            dashboardId,
            newLock,
            (key, existing) =>
            {
                if (existing.UserId == userId || DateTime.UtcNow - existing.LastActivity > LockTimeout)
                {
                    return newLock;
                }
                return existing;
            }
        );

        var success = acquired.UserId == userId;
        if (success)
        {
            _logger.LogInformation("Edit lock acquired for dashboard {DashboardId} by user {UserId} ({UserName})",
                dashboardId, userId, userName);
            OnLockChanged?.Invoke(dashboardId, acquired);
        }
        else
        {
            _logger.LogInformation("Edit lock denied for dashboard {DashboardId} by user {UserId} ({UserName}) - currently held by {CurrentUserId} ({CurrentUserName})",
                dashboardId, userId, userName, acquired.UserId, acquired.UserName);
        }

        return success;
    }

    /// <summary>
    /// Forcibly acquires an edit lock, overriding any existing lock.
    /// Use with caution - this will disconnect another user's edit session.
    /// </summary>
    public bool ForceAcquireLock(int dashboardId, string userId, string userName, string connectionId)
    {
        var previousLock = GetCurrentLock(dashboardId);

        var newLock = new EditLockInfo
        {
            UserId = userId,
            UserName = userName,
            LockedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow,
            ConnectionId = connectionId
        };

        _dashboardLocks.AddOrUpdate(dashboardId, newLock, (key, existing) => newLock);

        if (previousLock != null && previousLock.UserId != userId)
        {
            _logger.LogWarning("Edit lock forcefully acquired for dashboard {DashboardId} by user {UserId} ({UserName}), overriding lock held by {PreviousUserId} ({PreviousUserName})",
                dashboardId, userId, userName, previousLock.UserId, previousLock.UserName);
        }
        else
        {
            _logger.LogInformation("Edit lock forcefully acquired for dashboard {DashboardId} by user {UserId} ({UserName})",
                dashboardId, userId, userName);
        }

        OnLockChanged?.Invoke(dashboardId, newLock);

        return true;
    }

    /// <summary>
    /// Releases an edit lock if held by the specified user.
    /// </summary>
    public bool ReleaseLock(int dashboardId, string userId)
    {
        var removed = _dashboardLocks.TryGetValue(dashboardId, out var existing) &&
                     existing.UserId == userId &&
                     _dashboardLocks.TryRemove(dashboardId, out _);

        if (removed)
        {
            _logger.LogInformation("Edit lock released for dashboard {DashboardId} by user {UserId}", dashboardId, userId);
            OnLockChanged?.Invoke(dashboardId, null);
        }

        return removed;
    }

    /// <summary>
    /// Updates the last activity timestamp for an active lock.
    /// This prevents the lock from expiring due to inactivity.
    /// </summary>
    public void UpdateActivity(int dashboardId, string userId)
    {
        if (_dashboardLocks.TryGetValue(dashboardId, out var existing) && existing.UserId == userId)
        {
            existing.LastActivity = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Checks if a dashboard is locked by a different user.
    /// </summary>
    public bool IsLockedByOtherUser(int dashboardId, string userId)
    {
        if (!_dashboardLocks.TryGetValue(dashboardId, out var lockInfo))
            return false;

        return lockInfo.UserId != userId && DateTime.UtcNow - lockInfo.LastActivity <= LockTimeout;
    }

    /// <summary>
    /// Returns all currently active locks for monitoring purposes.
    /// </summary>
    public IReadOnlyDictionary<int, EditLockInfo> GetAllActiveLocks()
    {
        return _dashboardLocks
            .Where(kvp => DateTime.UtcNow - kvp.Value.LastActivity <= LockTimeout)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private void CleanupExpiredLocks(object? state)
    {
        var expiredLocks = _dashboardLocks
            .Where(kvp => DateTime.UtcNow - kvp.Value.LastActivity > LockTimeout)
            .ToList();

        foreach (var expiredLock in expiredLocks)
        {
            if (_dashboardLocks.TryRemove(expiredLock.Key, out var removedLock))
            {
                _logger.LogInformation("Expired edit lock cleaned up for dashboard {DashboardId} (was held by {UserId})",
                    expiredLock.Key, removedLock.UserId);
                OnLockChanged?.Invoke(expiredLock.Key, null);
            }
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}
