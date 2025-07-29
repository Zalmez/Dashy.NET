using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Dashy.Net.Shared.Models;

namespace Dashy.Net.Web.Services;

public enum ItemSize { Small, Medium, Large }
public enum LayoutOrientation { Vertical, Auto }

/// <summary>
/// Client-specific view options service. This service manages UI state that should be 
/// independent per browser tab/client (like search terms, item size, layout).
/// Edit mode and locks are handled separately to ensure proper multi-user coordination.
/// </summary>
public class ViewOptionsService
{
    private readonly EditLockService _editLockService;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<ViewOptionsService> _logger;
    private int? _currentDashboardId;
    private string _currentUserId = string.Empty;
    private string _currentUserName = string.Empty;
    private string _connectionId = Guid.NewGuid().ToString();


    public event Action? OnChange;
    public event Action<EditLockInfo?>? OnEditLockChanged;

    public bool IsEditMode { get; private set; } = false;

    public ItemSize CurrentItemSize { get; private set; } = ItemSize.Medium;
    public string SearchTerm { get; private set; } = string.Empty;
    public LayoutOrientation CurrentLayout { get; private set; } = LayoutOrientation.Auto;

    public EditLockInfo? CurrentEditLock { get; private set; }
    public bool CanEdit => CurrentEditLock?.UserId == _currentUserId;

    public ViewOptionsService(
        EditLockService editLockService,
        AuthenticationStateProvider authStateProvider,
        ILogger<ViewOptionsService> logger)
    {
        _editLockService = editLockService;
        _authStateProvider = authStateProvider;
        _logger = logger;
        _editLockService.OnLockChanged += OnLockChanged;

        _logger.LogDebug("ViewOptionsService created for new client session with connection ID: {ConnectionId}", _connectionId);
    }

    public async Task InitializeAsync(int? dashboardId = null)
    {
        var previousDashboardId = _currentDashboardId;
        _currentDashboardId = dashboardId;

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        _currentUserId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        _currentUserName = authState.User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous User";

        _logger.LogDebug("ViewOptionsService initialized for user {UserId} ({UserName}) on dashboard {DashboardId}",
            _currentUserId, _currentUserName, dashboardId);

        if (previousDashboardId.HasValue && previousDashboardId != dashboardId && IsEditMode)
        {
            _editLockService.ReleaseLock(previousDashboardId.Value, _currentUserId);
            IsEditMode = false;
            _logger.LogInformation("Released edit lock for previous dashboard {PreviousDashboardId} when switching to {NewDashboardId}",
                previousDashboardId.Value, dashboardId);
        }

        if (_currentDashboardId.HasValue)
        {
            CurrentEditLock = _editLockService.GetCurrentLock(_currentDashboardId.Value);
        }

        NotifyStateChanged();
    }

    public Task<bool> TryToggleEditModeAsync(bool forceAcquire = false)
    {
        if (!_currentDashboardId.HasValue)
        {
            _logger.LogWarning("Cannot toggle edit mode without a dashboard ID");
            return Task.FromResult(false);
        }

        if (!IsEditMode)
        {
            bool lockAcquired;

            if (forceAcquire)
            {
                lockAcquired = _editLockService.ForceAcquireLock(_currentDashboardId.Value, _currentUserId, _currentUserName, _connectionId);
            }
            else
            {
                lockAcquired = _editLockService.TryAcquireLock(_currentDashboardId.Value, _currentUserId, _currentUserName, _connectionId);
            }

            if (lockAcquired)
            {
                IsEditMode = true;
                NotifyStateChanged();
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
        else
        {
            _editLockService.ReleaseLock(_currentDashboardId.Value, _currentUserId);
            IsEditMode = false;
            NotifyStateChanged();
            return Task.FromResult(true);
        }
    }

    public void UpdateActivity()
    {
        if (_currentDashboardId.HasValue && IsEditMode)
        {
            _editLockService.UpdateActivity(_currentDashboardId.Value, _currentUserId);
        }
    }

    private void OnLockChanged(int dashboardId, EditLockInfo? lockInfo)
    {
        if (_currentDashboardId == dashboardId)
        {
            CurrentEditLock = lockInfo;

            if (IsEditMode && (lockInfo == null || lockInfo.UserId != _currentUserId))
            {
                IsEditMode = false;
                _logger.LogInformation("Exiting edit mode for dashboard {DashboardId} - lock lost or taken by another user", dashboardId);
            }

            OnEditLockChanged?.Invoke(lockInfo);
            NotifyStateChanged();
        }
    }

    /// <summary>
    /// Sets the search term for this client only. This will NOT sync across other clients.
    /// </summary>
    public void SetSearchTerm(string term)
    {
        if (SearchTerm == term) return;
        SearchTerm = term;
        _logger.LogDebug("Search term updated for client {ConnectionId}: '{SearchTerm}'", _connectionId, term);
        NotifyStateChanged();
    }

    /// <summary>
    /// Sets the item size preference for this client only. This will NOT sync across other clients.
    /// </summary>
    public void SetItemSize(ItemSize size)
    {
        if (CurrentItemSize == size) return;
        CurrentItemSize = size;
        _logger.LogDebug("Item size updated for client {ConnectionId}: {ItemSize}", _connectionId, size);
        NotifyStateChanged();
    }

    /// <summary>
    /// Sets the layout orientation for this client only. This will NOT sync across other clients.
    /// </summary>
    public void SetLayout(LayoutOrientation layout)
    {
        if (CurrentLayout == layout) return;
        CurrentLayout = layout;
        _logger.LogDebug("Layout orientation updated for client {ConnectionId}: {Layout}", _connectionId, layout);
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        try
        {
            OnChange?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying state change in ViewOptionsService");
        }
    }

    public void Dispose()
    {
        _editLockService.OnLockChanged -= OnLockChanged;

        if (_currentDashboardId.HasValue && IsEditMode)
        {
            _editLockService.ReleaseLock(_currentDashboardId.Value, _currentUserId);
        }
    }
}