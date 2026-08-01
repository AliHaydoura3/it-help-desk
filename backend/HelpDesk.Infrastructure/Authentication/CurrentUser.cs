using System.Security.Claims;
using HelpDesk.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace HelpDesk.Infrastructure.Authentication;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor)
    : ICurrentUser
{
    private ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException("No authenticated user context exists.");

    public Guid UserId
    {
        get
        {
            var value = Principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? Principal.FindFirstValue("sub");
            return Guid.TryParse(value, out var userId)
                ? userId
                : throw new UnauthorizedAccessException("The authenticated user identifier is invalid.");
        }
    }

    public string? Email => Principal.FindFirstValue(ClaimTypes.Email)
        ?? Principal.FindFirstValue("email");

    public bool IsInRole(string role) => Principal.IsInRole(role);
}
