using System.ComponentModel.DataAnnotations;

namespace HelpDesk.Infrastructure.Identity.Seeding;

public sealed class SeedAdminOptions
{
    public const string SectionName = "Admin";

    [Required]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    [Required]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    public string LastName { get; init; } = string.Empty;
}