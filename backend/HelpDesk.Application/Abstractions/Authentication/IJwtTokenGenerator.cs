using HelpDesk.Application.Authentication;

namespace HelpDesk.Application.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(UserIdentity user);
}