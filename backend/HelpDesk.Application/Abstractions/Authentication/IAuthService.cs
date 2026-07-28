using HelpDesk.Application.DTOs.Authentication;

namespace HelpDesk.Application.Abstractions.Authentication;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}