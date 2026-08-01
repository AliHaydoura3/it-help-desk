namespace HelpDesk.Application.Features.Users.GetUsers;

public sealed record GetUsersResponse(
    IReadOnlyList<GetUserResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    int ActiveCount,
    int InactiveCount,
    int AdministratorCount);
