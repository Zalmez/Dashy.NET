using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Dashy.Net.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace Dashy.Net.ApiService.Services;

public class AuthenticationSettingsProvider
{
    private readonly AppDbContext _dbContext;
    private AuthenticationSettings? _cachedSettings;
    private DateTime _lastLoad = DateTime.MinValue;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(1); // Shorter cache duration for settings

    public AuthenticationSettingsProvider(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuthenticationSettings?> GetSettingsAsync()
    {
        if (_cachedSettings != null && DateTime.UtcNow - _lastLoad < _cacheDuration)
            return _cachedSettings;

        var settings = await _dbContext.AuthenticationSettings.AsNoTracking().FirstOrDefaultAsync();
        if (settings != null && !string.IsNullOrEmpty(settings.ClientSecret))
        {
            settings.ClientSecret = AesEncryptionService.Decrypt(settings.ClientSecret);
        }
        _cachedSettings = settings;
        _lastLoad = DateTime.UtcNow;
        return settings;
    }

    public void ClearCache()
    {
        _cachedSettings = null;
        _lastLoad = DateTime.MinValue;
    }
}
