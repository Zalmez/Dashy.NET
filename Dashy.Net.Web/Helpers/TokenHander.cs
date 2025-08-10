using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace Dashy.Net.Web.Helpers;

public class TokenHandler(IHttpContextAccessor httpContextAccessor) :
    DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext is null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        try
        {
            var accessToken = await httpContextAccessor.HttpContext
                .GetTokenAsync("access_token");

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }
        catch (InvalidOperationException)
        {
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
