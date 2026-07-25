using Microsoft.AspNetCore.Identity;

namespace HelpDesk.Infrastructure.Identity.Seeding;

public sealed class RoleSeeder(RoleManager<ApplicationRole> roleManager)
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

    public async Task SeedAsync()
    {
        string[] roles =
        [
            "Admin",
            "IT Support Specialist",
            "Manager",
            "Employee"
        ];

        foreach(var role in roles)
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