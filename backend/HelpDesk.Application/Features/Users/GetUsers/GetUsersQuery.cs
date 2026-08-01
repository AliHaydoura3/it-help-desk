using MediatR;

namespace HelpDesk.Application.Features.Users.GetUsers;

public sealed record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    bool? IsActive = null) : IRequest<GetUsersResponse>
{
    public Guid CurrentUserId { get; init; }
}
