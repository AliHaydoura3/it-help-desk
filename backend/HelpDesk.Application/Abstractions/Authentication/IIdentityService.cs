using HelpDesk.Application.Common.Authentication;
using HelpDesk.Application.Features.Users.CreateUser;
using HelpDesk.Application.Features.Users.GetUsers;
using HelpDesk.Application.Features.Users.UpdateUser;
using HelpDesk.Application.Features.Profile.GetProfile;
using HelpDesk.Application.Features.Profile.UpdateProfile;

namespace HelpDesk.Application.Abstractions.Authentication;

public interface IIdentityService
{
    Task<UserIdentity?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<RefreshSession> CreateRefreshSessionAsync(
        UserIdentity user,
        CancellationToken cancellationToken = default);

    Task<RefreshSession?> RotateRefreshSessionAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeRefreshSessionAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task<string?> GeneratePasswordResetTokenAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<GetProfileResponse> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<UpdateProfileResponse> UpdateProfileAsync(
        UpdateProfileCommand command,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
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
