using HelpDesk.Application.Features.Users.GetUsers;
using MediatR;

namespace HelpDesk.Application.Features.Users.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<GetUserResponse>;
