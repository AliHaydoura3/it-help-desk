using HelpDesk.Application.Common.Authentication;
using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using HelpDesk.Application.Features.Users.CreateUser;
using HelpDesk.Application.Features.Users.GetUsers;
using HelpDesk.Application.Features.Users.UpdateUser;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager) : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;


    public async Task<UserIdentity?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordValid =
            await _userManager.CheckPasswordAsync(user, password);

        if (!passwordValid)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserIdentity(
            user.Id,
            user.Email!,
            roles);
    }

    public async Task<CreateUserResponse> CreateUserAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existingUser = await _userManager.FindByEmailAsync(command.Email);

        if (existingUser is not null)
            throw new InvalidOperationException("Email already exists.");

        var roles = command.Roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await EnsureRolesExistAsync(roles, cancellationToken);

        var user = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            EmailConfirmed = true,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(
            user,
            command.Password);

        if (!createResult.Succeeded)
            ThrowIdentityErrors(createResult);

        var roleResult = await _userManager.AddToRolesAsync(
            user,
            roles);

        if (!roleResult.Succeeded)
            ThrowIdentityErrors(roleResult);

        return new CreateUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!);
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
        var administrators = await _userManager.GetUsersInRoleAsync("Admin");
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

        var requestedRoles = command.Roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        await EnsureRolesExistAsync(requestedRoles, cancellationToken);

        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.Email = command.Email;
        user.UserName = command.Email;
        user.IsActive = command.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
            ThrowIdentityErrors(updateResult);

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(
            requestedRoles,
            StringComparer.OrdinalIgnoreCase);
        var rolesToAdd = requestedRoles.Except(
            currentRoles,
            StringComparer.OrdinalIgnoreCase);

        var removeResult = await _userManager.RemoveFromRolesAsync(
            user,
            rolesToRemove);

        if (!removeResult.Succeeded)
            ThrowIdentityErrors(removeResult);

        var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);

        if (!addResult.Succeeded)
            ThrowIdentityErrors(addResult);

        var roles = await _userManager.GetRolesAsync(user);

        return new UpdateUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.IsActive,
            roles.ToArray());
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

    private async Task EnsureRolesExistAsync(
        IEnumerable<string> roles,
        CancellationToken cancellationToken)
    {
        foreach (var role in roles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await _roleManager.RoleExistsAsync(role))
                throw new KeyNotFoundException(
                    $"Role '{role}' does not exist.");
        }
    }

    private async Task<GetUserResponse> MapUserAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        return new GetUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.IsActive,
            roles.ToArray());
    }

    private static void ThrowIdentityErrors(IdentityResult result)
    {
        var failures = result.Errors
            .Select(error => new ValidationFailure(
                string.Empty,
                error.Description));

        throw new FluentValidation.ValidationException(failures);
    }
}
