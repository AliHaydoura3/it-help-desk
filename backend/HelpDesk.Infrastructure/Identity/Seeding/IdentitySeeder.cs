using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDesk.Infrastructure.Identity.Seeding;

public sealed class IdentitySeeder(RoleSeeder roleSeeder, UserSeeder userSeeder)
{
    private readonly RoleSeeder _roleSeeder = roleSeeder;
    private readonly UserSeeder _userSeeder = userSeeder;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _roleSeeder.SeedAsync();

        await _userSeeder.SeedAsync();
    }
}