using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Application.DTOs.Authentication;
using HelpDesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace HelpDesk.Infrastructure.Authentication;

public class AuthService(UserManager<ApplicationUser> userManager, TokenService tokenService) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly TokenService _tokenService = tokenService;

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _tokenService.CreateAccessToken(user, roles);

        return new LoginResponse(accessToken);
    }
}