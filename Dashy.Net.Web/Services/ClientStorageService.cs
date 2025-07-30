using Microsoft.JSInterop;
using System.Text.Json;

namespace Dashy.Net.Web.Services;

/// <summary>
/// Service for managing client-side storage (localStorage) for user preferences
/// </summary>
public class ClientStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<ClientStorageService> _logger;
    private const string StoragePrefix = "dashy_";

    public ClientStorageService(IJSRuntime jsRuntime, ILogger<ClientStorageService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    /// <summary>
    /// Saves a value to localStorage
    /// </summary>
    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var storageKey = StoragePrefix + key;
            var jsonValue = JsonSerializer.Serialize(value);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", storageKey, jsonValue);
            _logger.LogDebug("Saved to localStorage: {Key} = {Value}", storageKey, jsonValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save item to localStorage: {Key}", key);
        }
    }

    /// <summary>
    /// Gets a value from localStorage
    /// </summary>
    public async Task<T?> GetItemAsync<T>(string key, T? defaultValue = default)
    {
        try
        {
            var storageKey = StoragePrefix + key;
            var jsonValue = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", storageKey);
            
            if (string.IsNullOrEmpty(jsonValue))
            {
                _logger.LogDebug("No value found in localStorage for key: {Key}, using default", storageKey);
                return defaultValue;
            }

            var value = JsonSerializer.Deserialize<T>(jsonValue);
            _logger.LogDebug("Retrieved from localStorage: {Key} = {Value}", storageKey, jsonValue);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get item from localStorage: {Key}, using default value", key);
            return defaultValue;
        }
    }

    /// <summary>
    /// Removes an item from localStorage
    /// </summary>
    public async Task RemoveItemAsync(string key)
    {
        try
        {
            var storageKey = StoragePrefix + key;
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", storageKey);
            _logger.LogDebug("Removed from localStorage: {Key}", storageKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove item from localStorage: {Key}", key);
        }
    }

    /// <summary>
    /// Checks if localStorage is available
    /// </summary>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "test", "test");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "test");
            return true;
        }
        catch
        {
            return false;
        }
    }
}
