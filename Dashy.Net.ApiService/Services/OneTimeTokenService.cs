using System.Security.Cryptography;
using System.Text;

namespace Dashy.Net.ApiService.Services;

/// <summary>
/// Provides one-time-use tokens that automatically regenerate after each use.
/// This service ensures secure access to authentication configuration endpoints.
/// </summary>
public class OneTimeTokenService
{
    private readonly ILogger<OneTimeTokenService> _logger;
    private readonly object _lockObject = new();
    private string _currentToken;
    private DateTime _tokenCreatedAt;
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromHours(1); // Token expires after 1 hour

    public OneTimeTokenService(ILogger<OneTimeTokenService> logger)
    {
        _logger = logger;
        _currentToken = GenerateNewToken();
        _tokenCreatedAt = DateTime.UtcNow;
        
        _logger.LogWarning("?? ONE-TIME TOKEN GENERATED: {Token}", _currentToken);
        _logger.LogWarning("?? Token expires at: {ExpiresAt}", GetTokenExpiration());
        _logger.LogWarning("?? Use this token as a query parameter to access authentication settings: ?token={Token}", _currentToken);
    }

    /// <summary>
    /// Gets the current one-time token without consuming it
    /// </summary>
    public string GetCurrentToken()
    {
        lock (_lockObject)
        {
            if (IsTokenExpired())
            {
                RegenerateToken();
            }
            return _currentToken;
        }
    }

    /// <summary>
    /// Gets the expiration time for the current token
    /// </summary>
    public DateTime GetTokenExpiration()
    {
        lock (_lockObject)
        {
            return _tokenCreatedAt.Add(_tokenLifetime);
        }
    }

    /// <summary>
    /// Validates the provided token and consumes it (regenerates a new one) if valid
    /// </summary>
    /// <param name="providedToken">The token to validate</param>
    /// <returns>True if the token was valid and has been consumed</returns>
    public bool ValidateAndConsumeToken(string? providedToken)
    {
        if (string.IsNullOrEmpty(providedToken))
            return false;

        lock (_lockObject)
        {
            // Check if token is expired
            if (IsTokenExpired())
            {
                _logger.LogWarning("?? One-time token validation failed: Token has expired");
                RegenerateToken();
                return false;
            }

            // Check if provided token matches current token
            if (!string.Equals(providedToken, _currentToken, StringComparison.Ordinal))
            {
                _logger.LogWarning("?? One-time token validation failed: Invalid token provided");
                return false;
            }

            // Token is valid - consume it by generating a new one
            _logger.LogInformation("? One-time token validated successfully. Generating new token.");
            RegenerateToken();
            return true;
        }
    }

    /// <summary>
    /// Forces regeneration of the token (useful for testing or manual refresh)
    /// </summary>
    public string RegenerateToken()
    {
        lock (_lockObject)
        {
            _currentToken = GenerateNewToken();
            _tokenCreatedAt = DateTime.UtcNow;
            
            _logger.LogWarning("?? NEW ONE-TIME TOKEN GENERATED: {Token}", _currentToken);
            _logger.LogWarning("?? Token expires at: {ExpiresAt}", GetTokenExpiration());
            
            return _currentToken;
        }
    }

    private bool IsTokenExpired()
    {
        return DateTime.UtcNow > _tokenCreatedAt.Add(_tokenLifetime);
    }

    private static string GenerateNewToken()
    {
        // Generate a cryptographically secure random token
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32]; // 256-bit token
        rng.GetBytes(bytes);
        
        // Convert to base64url (URL-safe base64)
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    /// <summary>
    /// Gets token statistics for monitoring/debugging
    /// </summary>
    public object GetTokenInfo()
    {
        lock (_lockObject)
        {
            return new
            {
                CurrentToken = _currentToken,
                CreatedAt = _tokenCreatedAt,
                ExpiresAt = GetTokenExpiration(),
                IsExpired = IsTokenExpired(),
                TimeRemaining = GetTokenExpiration() - DateTime.UtcNow
            };
        }
    }
}