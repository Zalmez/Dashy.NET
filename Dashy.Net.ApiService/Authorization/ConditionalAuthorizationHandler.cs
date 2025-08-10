using Microsoft.AspNetCore.Authorization;

namespace Dashy.Net.ApiService.Authorization;

public class ConditionalAuthorizationRequirement : IAuthorizationRequirement
{
    public bool AuthenticationRequired { get; }

    public ConditionalAuthorizationRequirement(bool authenticationRequired)
    {
        AuthenticationRequired = authenticationRequired;
    }
}

public class ConditionalAuthorizationHandler : AuthorizationHandler<ConditionalAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ConditionalAuthorizationRequirement requirement)
    {
        if (!requirement.AuthenticationRequired)
        {
            context.Succeed(requirement);
        }
        else if (context.User.Identity?.IsAuthenticated == true)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
