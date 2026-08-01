using HelpDesk.Application.Features.ActivityLogs.GetActivityLogs;

namespace HelpDesk.Application.Abstractions.Logging;

public interface IActivityLogReader
{
    Task<GetActivityLogsResponse> GetAsync(
        GetActivityLogsQuery query,
        CancellationToken cancellationToken = default);
}
