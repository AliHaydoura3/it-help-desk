using HelpDesk.Application.Common.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Authorization;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddApplicationAuthorization(
        this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                Policies.AdminOnly,
                policy => policy.RequireRole(Roles.Admin));
            options.AddPolicy(
                Policies.Reporting,
                policy => policy.RequireRole(Roles.Admin, Roles.Manager));
        });

        return services;
    }
}
