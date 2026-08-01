using System.Security.Claims;
using HelpDesk.Application.Common.Authorization;
using HelpDesk.Application.Features.Users.CreateUser;
using HelpDesk.Application.Features.Users.DeleteUser;
using HelpDesk.Application.Features.Users.GetUserById;
using HelpDesk.Application.Features.Users.GetUsers;
using HelpDesk.Application.Features.Users.UpdateUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.AdminOnly)]
public sealed class UsersController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpPost]
    public async Task<ActionResult<CreateUserResponse>> Create(
        [FromBody] CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [HttpGet]
    public async Task<ActionResult<GetUsersResponse>> GetAll(
        [FromQuery] GetUsersQuery query,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            query with { CurrentUserId = GetCurrentUserId() },
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetUserResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetUserByIdQuery(id),
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateUserResponse>> Update(
        Guid id,
        [FromBody] UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            command with
            {
                Id = id,
                CurrentUserId = GetCurrentUserId()
            },
            cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteUserCommand(id, GetCurrentUserId()),
            cancellationToken);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException(
                "The authenticated user identifier is invalid.");
    }
}
