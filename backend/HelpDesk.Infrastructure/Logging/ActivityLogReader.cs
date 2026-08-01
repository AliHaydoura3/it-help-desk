using HelpDesk.Application.Abstractions.Logging;
using HelpDesk.Application.Features.ActivityLogs.GetActivityLogs;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Logging;

public sealed class ActivityLogReader(ApplicationDbContext dbContext)
    : IActivityLogReader
{
    public async Task<GetActivityLogsResponse> GetAsync(
        GetActivityLogsQuery query,
        CancellationToken cancellationToken = default)
    {
        var logs = dbContext.UserActivityLogs.AsNoTracking();
        var totalCount = await logs.CountAsync(cancellationToken);
        var items = await logs
            .OrderByDescending(log => log.OccurredAtUtc)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(log => new ActivityLogResponse(
                log.Id,
                log.UserId,
                log.UserEmail,
                log.Action,
                log.Resource,
                log.ResourceId,
                log.IpAddress,
                log.Succeeded,
                log.OccurredAtUtc))
            .ToListAsync(cancellationToken);

        return new GetActivityLogsResponse(
            items,
            query.PageNumber,
            query.PageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)query.PageSize));
    }
}
