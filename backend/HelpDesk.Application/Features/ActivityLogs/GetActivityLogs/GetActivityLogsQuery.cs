using MediatR;

namespace HelpDesk.Application.Features.ActivityLogs.GetActivityLogs;

public sealed record GetActivityLogsQuery(
    int PageNumber = 1,
    int PageSize = 20) : IRequest<GetActivityLogsResponse>;
