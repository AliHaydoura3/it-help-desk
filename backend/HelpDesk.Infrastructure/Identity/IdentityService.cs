using System.Security.Cryptography;
using System.Text;
using HelpDesk.Application.Common.Authentication;
using HelpDesk.Application.Common.Authorization;
using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using HelpDesk.Application.Features.Users.CreateUser;
using HelpDesk.Application.Features.Users.GetUsers;
using HelpDesk.Application.Features.Users.UpdateUser;
using HelpDesk.Application.Features.Profile.GetProfile;
using HelpDesk.Application.Features.Profile.UpdateProfile;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Infrastructure.Authentication.Jwt;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Domain;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ApplicationDbContext dbContext,
    IOptions<JwtOptions> jwtOptions) : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;


    public async Task<UserIdentity?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive ||
            await _userManager.IsLockedOutAsync(user))
        {
            return null;
        }

        var passwordValid =
            await _userManager.CheckPasswordAsync(user, password);

        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user);
            return null;
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var role = await GetSingleRoleAsync(user);

        return new UserIdentity(
            user.Id,
            user.Email!,
            role);
    }

    public async Task<RefreshSession> CreateRefreshSessionAsync(
        UserIdentity user,
        CancellationToken cancellationToken = default)
    {
        var (refreshToken, entity) = CreateRefreshToken(user.Id);

        _dbContext.RefreshTokens.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshSession(
            user,
            refreshToken,
            entity.ExpiresAtUtc);
    }

    public async Task<RefreshSession?> RotateRefreshSessionAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var tokenHash = HashToken(refreshToken);
        var existingToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        if (existingToken is null ||
            existingToken.RevokedAtUtc.HasValue ||
            existingToken.ExpiresAtUtc <= now)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(
            existingToken.UserId.ToString());

        if (user is null || !user.IsActive)
            return null;

        var role = await GetSingleRoleAsync(user);
        var identity = new UserIdentity(user.Id, user.Email!, role);
        var (newToken, newEntity) = CreateRefreshToken(user.Id);

        existingToken.RevokedAtUtc = now;
        existingToken.ReplacedByTokenId = newEntity.Id;
        _dbContext.RefreshTokens.Add(newEntity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshSession(
            identity,
            newToken,
            newEntity.ExpiresAtUtc);
    }

    public async Task RevokeRefreshSessionAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);
        var token = await _dbContext.RefreshTokens.SingleOrDefaultAsync(
            storedToken => storedToken.TokenHash == tokenHash,
            cancellationToken);

        if (token is null || token.RevokedAtUtc.HasValue)
            return;

        token.RevokedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
            return null;

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task ResetPasswordAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByEmailAsync(email)
            ?? throw new FluentValidation.ValidationException(
                "The reset link is invalid or has expired.");
        var result = await _userManager.ResetPasswordAsync(
            user,
            token,
            newPassword);

        if (!result.Succeeded)
            ThrowIdentityErrors(result);

        var activeTokens = await _dbContext.RefreshTokens
            .Where(refreshToken =>
                refreshToken.UserId == user.Id &&
                !refreshToken.RevokedAtUtc.HasValue)
            .ToListAsync(cancellationToken);

        foreach (var activeToken in activeTokens)
            activeToken.RevokedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<GetProfileResponse> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindUserByIdAsync(userId);
        var role = await GetSingleRoleAsync(user);

        return new GetProfileResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            role);
    }

    public async Task<UpdateProfileResponse> UpdateProfileAsync(
        UpdateProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindUserByIdAsync(command.UserId);
        var userWithEmail = await _userManager.FindByEmailAsync(command.Email);

        if (userWithEmail is not null && userWithEmail.Id != user.Id)
            throw new InvalidOperationException("Email already exists.");

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.Email = command.Email;
        user.UserName = command.Email;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            ThrowIdentityErrors(result);

        var role = await GetSingleRoleAsync(user);

        return new UpdateProfileResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            role);
    }

    public async Task ChangePasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindUserByIdAsync(userId);
        var result = await _userManager.ChangePasswordAsync(
            user,
            currentPassword,
            newPassword);

        if (!result.Succeeded)
            ThrowIdentityErrors(result);

        var tokens = await _dbContext.RefreshTokens
            .Where(token =>
                token.UserId == userId &&
                !token.RevokedAtUtc.HasValue)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
            token.RevokedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CreateUserResponse> CreateUserAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingUser = await _userManager.FindByEmailAsync(command.Email);

        if (existingUser is not null)
            throw new InvalidOperationException("Email already exists.");

        var role = GetCanonicalRole(command.Role);
        await EnsureRoleExistsAsync(role, cancellationToken);

        var user = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            EmailConfirmed = true,
            IsActive = true
        };

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var createResult = await _userManager.CreateAsync(user, command.Password);

        if (!createResult.Succeeded)
            ThrowIdentityErrors(createResult);

        var roleResult = await _userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
            ThrowIdentityErrors(roleResult);

        var response = new CreateUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            role);

        await transaction.CommitAsync(cancellationToken);

        return response;
    }

    public async Task<GetUsersResponse> GetUsersAsync(
        GetUsersQuery request,
        CancellationToken cancellationToken = default)
    {
        var allUsers = _userManager.Users
            .AsNoTracking()
            .Where(user => user.Id != request.CurrentUserId);

        var activeCount = await allUsers.CountAsync(
            user => user.IsActive,
            cancellationToken);
        var inactiveCount = await allUsers.CountAsync(
            user => !user.IsActive,
            cancellationToken);
        var administrators = await _userManager.GetUsersInRoleAsync(Roles.Admin);
        var administratorCount = administrators.Count(
            user => user.Id != request.CurrentUserId);

        var filteredUsers = allUsers;

        if (request.IsActive.HasValue)
        {
            filteredUsers = filteredUsers.Where(
                user => user.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            filteredUsers = filteredUsers.Where(user =>
                user.FirstName.Contains(search) ||
                user.LastName.Contains(search) ||
                (user.FirstName + " " + user.LastName).Contains(search) ||
                (user.Email != null && user.Email.Contains(search)));
        }

        var totalCount = await filteredUsers.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(
            totalCount / (double)request.PageSize);
        var users = await filteredUsers
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var responses = new List<GetUserResponse>(users.Count);

        foreach (var user in users)
        {
            cancellationToken.ThrowIfCancellationRequested();
            responses.Add(await MapUserAsync(user));
        }

        return new GetUsersResponse(
            responses,
            request.PageNumber,
            request.PageSize,
            totalCount,
            totalPages,
            activeCount,
            inactiveCount,
            administratorCount);
    }

    public async Task<GetUserResponse> GetUserByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await FindUserByIdAsync(id);

        return await MapUserAsync(user);
    }

    public async Task<UpdateUserResponse> UpdateUserAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (command.Id == command.CurrentUserId && !command.IsActive)
            throw new InvalidOperationException(
                "You cannot deactivate your own account.");

        var user = await FindUserByIdAsync(command.Id);
        var userWithEmail = await _userManager.FindByEmailAsync(command.Email);

        if (userWithEmail is not null && userWithEmail.Id != user.Id)
            throw new InvalidOperationException("Email already exists.");

        var requestedRole = GetCanonicalRole(command.Role);
        await EnsureRoleExistsAsync(requestedRole, cancellationToken);

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.Email = command.Email;
        user.UserName = command.Email;
        user.IsActive = command.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            ThrowIdentityErrors(updateResult);

        var currentRole = await GetSingleRoleAsync(user);

        if (!currentRole.Equals(requestedRole, StringComparison.OrdinalIgnoreCase))
        {
            var removeResult = await _userManager.RemoveFromRoleAsync(user, currentRole);

            if (!removeResult.Succeeded)
                ThrowIdentityErrors(removeResult);

            var addResult = await _userManager.AddToRoleAsync(user, requestedRole);

            if (!addResult.Succeeded)
                ThrowIdentityErrors(addResult);
        }

        var response = new UpdateUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.IsActive,
            requestedRole);

        await transaction.CommitAsync(cancellationToken);

        return response;
    }

    public async Task DeleteUserAsync(
        Guid id,
        Guid currentUserId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (id == currentUserId)
            throw new InvalidOperationException(
                "You cannot deactivate your own account.");

        var user = await FindUserByIdAsync(id);
        user.IsActive = false;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            ThrowIdentityErrors(updateResult);
    }

    private async Task<ApplicationUser> FindUserByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        return user ?? throw new KeyNotFoundException(
            $"User '{id}' does not exist.");
    }

    private async Task EnsureRoleExistsAsync(
        string role,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!await _roleManager.RoleExistsAsync(role))
            throw new KeyNotFoundException($"Role '{role}' does not exist.");
    }

    private static string GetCanonicalRole(string role) =>
        Roles.All.Single(knownRole =>
            knownRole.Equals(role, StringComparison.OrdinalIgnoreCase));

    private async Task<GetUserResponse> MapUserAsync(ApplicationUser user)
    {
        var role = await GetSingleRoleAsync(user);

        return new GetUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.IsActive,
            role);
    }

    private async Task<string> GetSingleRoleAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return roles.Count switch
        {
            1 => roles[0],
            0 => throw new InvalidOperationException(
                $"User '{user.Id}' does not have an assigned role."),
            _ => throw new InvalidOperationException(
                $"User '{user.Id}' has multiple roles. Each user must have exactly one role.")
        };
    }

    private static void ThrowIdentityErrors(IdentityResult result)
    {
        var failures = result.Errors
            .Select(error => new ValidationFailure(
                string.Empty,
                error.Description));

        throw new FluentValidation.ValidationException(failures);
    }

    private (string RawToken, RefreshToken Entity) CreateRefreshToken(
        Guid userId)
    {
        var rawToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(64))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
        var now = DateTime.UtcNow;

        return (
            rawToken,
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = HashToken(rawToken),
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddDays(
                    _jwtOptions.RefreshTokenExpiryDays)
            });
    }

    private static string HashToken(string token)
    {
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
