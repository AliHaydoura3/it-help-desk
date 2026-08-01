using MediatR;

namespace HelpDesk.Application.Features.Profile.GetProfile;

public sealed record GetProfileQuery(Guid UserId)
    : IRequest<GetProfileResponse>;
