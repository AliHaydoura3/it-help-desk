using MediatR;

namespace HelpDesk.Application.Features.Profile.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword) : IRequest
{
    public Guid UserId { get; init; }
}
