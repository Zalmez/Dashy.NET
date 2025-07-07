using System.Net.Http.Json;
using Dashy.Net.Shared.Models;

namespace Dashy.Net.Web.Clients;

public class SettingsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SettingsClient> _logger;

    public AuthClient Auth { get; }

    public SettingsClient(HttpClient httpClient, ILoggerFactory loggerFactory)
    {
        _httpClient = httpClient;
        _logger = loggerFactory.CreateLogger<SettingsClient>();

        Auth = new AuthClient(httpClient, loggerFactory.CreateLogger<AuthClient>());
    }

    public class AuthClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AuthClient> _logger;

        public ProvidersClient Providers { get; }
        public TemplatesClient Templates { get; }

        public AuthClient(HttpClient httpClient, ILogger<AuthClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            Providers = new ProvidersClient(httpClient, logger);
            Templates = new TemplatesClient(httpClient, logger);
        }

        public async Task<AuthenticationSettings?> GetAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<AuthenticationSettings>("api/auth-settings");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch authentication settings.");
                return null;
            }
        }

        public async Task<bool> UpdateAsync(AuthenticationSettings settings)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync("api/auth-settings", settings);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update authentication settings.");
                return false;
            }
        }

        public async Task<IEnumerable<AuthenticationProviderTemplate>> GetAllAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<AuthenticationProviderTemplate>>("api/auth-providers/templates") ?? Enumerable.Empty<AuthenticationProviderTemplate>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch authentication provider templates.");
                return Enumerable.Empty<AuthenticationProviderTemplate>();
            }
        }

        public class ProvidersClient
        {
            private readonly HttpClient _httpClient;
            private readonly ILogger<AuthClient> _logger;

            public ProvidersClient(HttpClient httpClient, ILogger<AuthClient> logger)
            {
                _httpClient = httpClient;
                _logger = logger;
            }

            public async Task<IEnumerable<AuthenticationProviderVm>> GetAllAsync()
            {
                try
                {
                    return await _httpClient.GetFromJsonAsync<IEnumerable<AuthenticationProviderVm>>("api/auth-providers") ?? Enumerable.Empty<AuthenticationProviderVm>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch authentication providers.");
                    return Enumerable.Empty<AuthenticationProviderVm>();
                }
            }

            public async Task<bool> DeleteAsync(int id)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync($"api/auth-providers/{id}");
                    return response.IsSuccessStatusCode;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete authentication provider {Id}", id);
                    return false;
                }
            }
        }

        public class TemplatesClient
        {
            private readonly HttpClient _httpClient;
            private readonly ILogger<AuthClient> _logger;

            public TemplatesClient(HttpClient httpClient, ILogger<AuthClient> logger)
            {
                _httpClient = httpClient;
                _logger = logger;
            }

            public async Task<IEnumerable<AuthenticationProviderTemplate>> GetAllAsync()
            {
                try
                {
                    return await _httpClient.GetFromJsonAsync<IEnumerable<AuthenticationProviderTemplate>>("api/auth-providers/templates") ?? Enumerable.Empty<AuthenticationProviderTemplate>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch authentication provider templates.");
                    return Enumerable.Empty<AuthenticationProviderTemplate>();
                }
            }
        }
    }
}