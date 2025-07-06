using Dashy.Net.Shared.Data;
using Dashy.Net.Shared.Models;
using Microsoft.AspNetCore.Http.HttpResults;
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
        try
        {
            if (_cachedSettings != null && DateTime.UtcNow - _lastLoad < _cacheDuration)
                return _cachedSettings;

            var settings = await _dbContext.AuthenticationSettings.AsNoTracking().FirstOrDefaultAsync();
            _cachedSettings = settings;
            _lastLoad = DateTime.UtcNow;
            return settings;

        }
        catch (Exception)
        {
            return new AuthenticationSettings { IsEnabled = false };
        }
    }

    public void ClearCache()
    {
        _cachedSettings = null;
        _lastLoad = DateTime.MinValue;
    }
}
