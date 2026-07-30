using HelpDesk.Application.Common.Authorization;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.Infrastructure.Identity.Seeding;

public sealed class RoleSeeder(RoleManager<ApplicationRole> roleManager)
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

    public async Task SeedAsync()
    {
        foreach(var role in Roles.All)
        {
            if (await _roleManager.RoleExistsAsync(role)) continue;

            var result = await _roleManager.CreateAsync(new ApplicationRole { Name = role });

            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create role '{role}'.");
            }
        }
    }
}