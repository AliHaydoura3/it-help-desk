using HelpDesk.Application.Common.Authentication;

namespace HelpDesk.Application.Abstractions.Authentication;

public interface IIdentityService
{
    Task<UserIdentity?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}