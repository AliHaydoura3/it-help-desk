using HelpDesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace HelpDesk.Infrastructure.Identity.Seeding;

public sealed class AdminSeeder(UserManager<ApplicationUser> userManager, IConfiguration configuration)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;

    public async Task SeedAsync()
    {
        string? email = _configuration["Admin:Email"];
        string? password = _configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Missing Admin data.");
        }

        var admin = await _userManager.FindByEmailAsync(email);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email.Trim(),
                FirstName = "System",
                LastName = "Admin",
                UserName = email.Trim(),
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(admin, password);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException("Unable to create admin.");
            }
        }

        if (!await _userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult = await _userManager.AddToRoleAsync(admin, "Admin");
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException("Unable to assign Admin role.");
            }
        }
    }
}