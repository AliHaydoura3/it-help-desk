using HelpDesk.Application.Common.Authentication;
using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserIdentity?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordValid =
            await _userManager.CheckPasswordAsync(user, password);

        if (!passwordValid)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserIdentity(
            user.Id,
            user.Email!,
            roles);
    }
}