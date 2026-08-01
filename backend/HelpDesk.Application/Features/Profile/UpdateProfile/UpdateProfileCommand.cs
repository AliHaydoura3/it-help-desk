using MediatR;

namespace HelpDesk.Application.Features.Profile.UpdateProfile;

public sealed record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string Email) : IRequest<UpdateProfileResponse>
{
    public Guid UserId { get; init; }
}
