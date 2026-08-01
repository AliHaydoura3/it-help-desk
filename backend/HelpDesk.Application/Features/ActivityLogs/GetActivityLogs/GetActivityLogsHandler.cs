using HelpDesk.Application.Abstractions.Logging;
using MediatR;

namespace HelpDesk.Application.Features.ActivityLogs.GetActivityLogs;

public sealed class GetActivityLogsHandler(IActivityLogReader reader)
    : IRequestHandler<GetActivityLogsQuery, GetActivityLogsResponse>
{
    public Task<GetActivityLogsResponse> Handle(
        GetActivityLogsQuery request,
        CancellationToken cancellationToken)
    {
        return reader.GetAsync(request, cancellationToken);
    }
}
