using System.Net.Http.Json;
using Dashy.Net.Shared.Models;

namespace Dashy.Net.Web.Services;

public class AuthenticationSettingsProvider (IHttpClientFactory httpClientFactory, ILogger<AuthenticationSettingsProvider> logger)
{
    private AuthenticationSettings? _cachedSettings;
    private DateTime _lastLoad = DateTime.MinValue;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);


    public async Task<AuthenticationSettings?> GetSettingsAsync()
    {
        if (_cachedSettings != null && DateTime.UtcNow - _lastLoad < _cacheDuration)
            return _cachedSettings;
        try
        {
            var client = httpClientFactory.CreateClient("ApiService");
            var settings = await client.GetFromJsonAsync<AuthenticationSettings>("api/auth-settings");
            _cachedSettings = settings;
            _lastLoad = DateTime.UtcNow;
            return settings;
        }
        catch (Exception ex )
        {
            logger.LogWarning(ex, "Failed to load authentication settings. NOTE: if authentication is yet enabled ignore this");
            _cachedSettings = null;
            _lastLoad = DateTime.UtcNow;
            return null;
        }
    }
}
