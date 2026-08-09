using HelpDesk.Application.Common.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Identity.Seeding;

public sealed class UserSeeder(UserManager<ApplicationUser> userManager, IOptions<SeedAdminOptions> options)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SeedAdminOptions _options = options.Value;

    public async Task SeedAsync()
    {
        var admin = await _userManager.FindByEmailAsync(_options.Email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = _options.Email.Trim(),
                FirstName = _options.FirstName,
                LastName = _options.LastName,
                UserName = _options.Email.Trim(),
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(admin, _options.Password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed admin user.{Environment.NewLine}" +
                    string.Join(Environment.NewLine,
                        createResult.Errors.Select(e => e.Description)));
            }

        }

        var currentRoles = await _userManager.GetRolesAsync(admin);

        if (currentRoles.Count != 1 || !currentRoles[0].Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase))
        {
            if (currentRoles.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(admin, currentRoles);
                if (!removeResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to normalize the seeded administrator role.{Environment.NewLine}" +
                        string.Join(Environment.NewLine,
                            removeResult.Errors.Select(e => e.Description)));
                }
            }

            var roleResult = await _userManager.AddToRoleAsync(admin, Roles.Admin);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign '{Roles.Admin}' role.{Environment.NewLine}" +
                    string.Join(Environment.NewLine,
                        roleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}
