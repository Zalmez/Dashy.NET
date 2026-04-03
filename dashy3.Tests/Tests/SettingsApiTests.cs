using dashy3.Tests.Infrastructure;

namespace dashy3.Tests.Tests;

[Collection("Aspire")]
public class SettingsApiTests(AspireFixture fixture)
{
    [Fact]
    public async Task GetOidcConfig_ReturnsDefaults()
    {
        var client = fixture.CreateApiClient();
        var response = await client.GetAsync("/api/settings/oidc");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var config = await response.Content.ReadFromJsonAsync<OidcConfigDto>();
        Assert.NotNull(config);
        Assert.Empty(config.ClientSecret); // Secret never returned
    }

    [Fact]
    public async Task PutOidcConfig_SavesAndRedactsSecret()
    {
        var client = fixture.CreateApiClient();

        var putResponse = await client.PutAsJsonAsync("/api/settings/oidc", new
        {
            authority = "https://auth.example.com",
            clientId = "dashy3-test",
            clientSecret = "super-secret-value",
            scopes = "openid profile email",
            callbackPath = "/signin-oidc",
            isEnabled = false
        });
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        // Re-fetch and verify secret is hidden
        var getResponse = await client.GetAsync("/api/settings/oidc");
        var config = await getResponse.Content.ReadFromJsonAsync<OidcConfigDto>();
        Assert.NotNull(config);
        Assert.Equal("dashy3-test", config.ClientId);
        Assert.Equal("https://auth.example.com", config.Authority);
        Assert.Empty(config.ClientSecret); // Must never be returned
    }

    [Fact]
    public async Task PutOidcConfig_IsEnabledRoundTrips()
    {
        var client = fixture.CreateApiClient();

        await client.PutAsJsonAsync("/api/settings/oidc", new
        {
            authority = "https://sso.example.org",
            clientId = "dashy3",
            clientSecret = (string?)null,
            scopes = "openid profile email",
            callbackPath = "/signin-oidc",
            isEnabled = true
        });

        var getResponse = await client.GetAsync("/api/settings/oidc");
        var config = await getResponse.Content.ReadFromJsonAsync<OidcConfigDto>();
        Assert.True(config?.IsEnabled);
    }

    private record OidcConfigDto(string Authority, string ClientId, string ClientSecret,
        string Scopes, string CallbackPath, bool IsEnabled, DateTime UpdatedAt);
}
