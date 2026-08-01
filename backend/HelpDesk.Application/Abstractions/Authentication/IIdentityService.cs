using HelpDesk.Application.Common.Authentication;
using HelpDesk.Application.Features.Users.CreateUser;
using HelpDesk.Application.Features.Users.GetUsers;
using HelpDesk.Application.Features.Users.UpdateUser;

namespace HelpDesk.Application.Abstractions.Authentication;

public interface IIdentityService
{
    Task<UserIdentity?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<CreateUserResponse> CreateUserAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default);

    Task<GetUsersResponse> GetUsersAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default);

    Task<GetUserResponse> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<UpdateUserResponse> UpdateUserAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default);

    Task DeleteUserAsync(
        Guid id,
        Guid currentUserId,
        CancellationToken cancellationToken = default);
}
