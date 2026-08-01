using System.Security.Claims;
using HelpDesk.Application.Features.Profile.ChangePassword;
using HelpDesk.Application.Features.Profile.GetProfile;
using HelpDesk.Application.Features.Profile.UpdateProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class ProfileController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GetProfileResponse>> Get(
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(
            new GetProfileQuery(GetCurrentUserId()),
            cancellationToken));
    }

    [HttpPut]
    public async Task<ActionResult<UpdateProfileResponse>> Update(
        [FromBody] UpdateProfileCommand command,
        CancellationToken cancellationToken)
    {
        return Ok(await sender.Send(
            command with { UserId = GetCurrentUserId() },
            cancellationToken));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            command with { UserId = GetCurrentUserId() },
            cancellationToken);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException();
    }
}
