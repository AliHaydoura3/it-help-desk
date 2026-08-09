using System.Globalization;
using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Application.Abstractions.Reporting;
using HelpDesk.Application.Common.Authorization;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Features.Reporting;
using HelpDesk.Application.Features.Reporting.AgentPerformance;
using HelpDesk.Application.Features.Reporting.Dashboard;
using HelpDesk.Application.Features.Reporting.EmployeeActivity;
using HelpDesk.Application.Features.Reporting.Monthly;
using HelpDesk.Application.Features.Reporting.Sla;
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Reporting;

public sealed class ReportingService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IOptions<ReportingOptions> options) : IReportingService
{
    private readonly ReportingOptions _options = options.Value;

    public async Task<DashboardReportResponse> GetDashboardAsync(
        GetDashboardReportQuery query,
        CancellationToken cancellationToken)
    {
        EnsureCanViewReports();
        var now = DateTime.UtcNow;
        var period = ResolvePeriod(query.FromUtc, query.ToUtc, now);
        var tickets = dbContext.Tickets.AsNoTracking()
            .Where(ticket => !ticket.IsCancelled);

        var statusCounts = await tickets
            .GroupBy(ticket => ticket.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Status, item => item.Count, cancellationToken);
        var categoryCounts = await tickets
            .GroupBy(ticket => ticket.Category)
            .Select(group => new { Category = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Category, item => item.Count, cancellationToken);
        var categories = Enum.GetValues<TicketCategory>()
            .Select(category => new CategoryMetricResponse(
                category,
                categoryCounts.GetValueOrDefault(category)))
            .ToList();
        var priorityCounts = await tickets
            .GroupBy(ticket => ticket.Priority)
            .Select(group => new { Priority = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Priority, item => item.Count, cancellationToken);
        var priorities = Enum.GetValues<TicketPriority>()
            .Select(priority => new PriorityMetricResponse(
                priority,
                priorityCounts.GetValueOrDefault(priority)))
            .ToList();
        var resolvedDurations = await tickets
            .Where(ticket => ticket.ResolvedAtUtc >= period.FromUtc &&
                ticket.ResolvedAtUtc <= period.ToUtc)
            .Select(ticket => new { ticket.CreatedAtUtc, ticket.ResolvedAtUtc })
            .ToListAsync(cancellationToken);
        var performance = await BuildAgentPerformanceAsync(period, cancellationToken);
        var sla = await BuildSlaReportAsync(period, now, cancellationToken);

        return new DashboardReportResponse(
            now,
            period,
            statusCounts.Values.Sum(),
            GetCount(statusCounts, TicketStatus.Open),
            GetCount(statusCounts, TicketStatus.InProgress),
            GetCount(statusCounts, TicketStatus.Pending),
            GetCount(statusCounts, TicketStatus.Resolved),
            GetCount(statusCounts, TicketStatus.Closed),
            categories,
            priorities,
            performance,
            AverageHours(resolvedDurations.Select(item =>
                item.ResolvedAtUtc!.Value - item.CreatedAtUtc)),
            sla.Summary);
    }

    public async Task<AgentPerformanceReportResponse> GetAgentPerformanceAsync(
        GetAgentPerformanceReportQuery query,
        CancellationToken cancellationToken)
    {
        EnsureCanViewReports();
        var now = DateTime.UtcNow;
        var period = ResolvePeriod(query.FromUtc, query.ToUtc, now);
        return new AgentPerformanceReportResponse(
            now,
            period,
            await BuildAgentPerformanceAsync(period, cancellationToken));
    }

    public async Task<MonthlyTicketReportResponse> GetMonthlyReportAsync(
        GetMonthlyTicketReportQuery query,
        CancellationToken cancellationToken)
    {
        EnsureCanViewReports();
        var now = DateTime.UtcNow;
        var currentMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var fromUtc = currentMonth.AddMonths(-(query.Months - 1));
        var toExclusive = currentMonth.AddMonths(1);
        var ticketData = await dbContext.Tickets.AsNoTracking()
            .Where(ticket => ticket.CreatedAtUtc < toExclusive &&
                (ticket.CreatedAtUtc >= fromUtc ||
                    ticket.ResolvedAtUtc >= fromUtc ||
                    ticket.ClosedAtUtc >= fromUtc))
            .Select(ticket => new MonthlyTicketData(
                ticket.CreatedAtUtc,
                ticket.ResolvedAtUtc,
                ticket.ClosedAtUtc,
                ticket.IsCancelled))
            .ToListAsync(cancellationToken);

        var months = new List<MonthlyTicketMetricResponse>(query.Months);
        for (var monthStart = fromUtc; monthStart < toExclusive; monthStart = monthStart.AddMonths(1))
        {
            var monthEnd = monthStart.AddMonths(1);
            var resolved = ticketData.Where(ticket =>
                ticket.ResolvedAtUtc >= monthStart && ticket.ResolvedAtUtc < monthEnd).ToList();
            months.Add(new MonthlyTicketMetricResponse(
                monthStart.Year,
                monthStart.Month,
                monthStart.ToString("MMM yyyy", CultureInfo.InvariantCulture),
                ticketData.Count(ticket => ticket.CreatedAtUtc >= monthStart && ticket.CreatedAtUtc < monthEnd),
                resolved.Count,
                ticketData.Count(ticket => !ticket.IsCancelled &&
                    ticket.ClosedAtUtc >= monthStart && ticket.ClosedAtUtc < monthEnd),
                ticketData.Count(ticket => ticket.IsCancelled &&
                    ticket.ClosedAtUtc >= monthStart && ticket.ClosedAtUtc < monthEnd),
                AverageHours(resolved.Select(ticket =>
                    ticket.ResolvedAtUtc!.Value - ticket.CreatedAtUtc))));
        }

        var allResolved = ticketData.Where(ticket =>
            ticket.ResolvedAtUtc >= fromUtc && ticket.ResolvedAtUtc < toExclusive).ToList();
        return new MonthlyTicketReportResponse(
            now,
            new ReportingPeriodResponse(fromUtc, toExclusive.AddTicks(-1)),
            months.Sum(month => month.CreatedTickets),
            months.Sum(month => month.ResolvedTickets),
            months.Sum(month => month.ClosedTickets),
            months.Sum(month => month.CancelledTickets),
            AverageHours(allResolved.Select(ticket =>
                ticket.ResolvedAtUtc!.Value - ticket.CreatedAtUtc)),
            months);
    }

    public async Task<SlaReportResponse> GetSlaReportAsync(
        GetSlaReportQuery query,
        CancellationToken cancellationToken)
    {
        EnsureCanViewReports();
        var now = DateTime.UtcNow;
        return await BuildSlaReportAsync(
            ResolvePeriod(query.FromUtc, query.ToUtc, now),
            now,
            cancellationToken);
    }

    public async Task<EmployeeActivityReportResponse> GetEmployeeActivityAsync(
        GetEmployeeActivityReportQuery query,
        CancellationToken cancellationToken)
    {
        EnsureCanViewReports();
        var now = DateTime.UtcNow;
        var period = ResolvePeriod(query.FromUtc, query.ToUtc, now);
        var usersQuery =
            from user in dbContext.Users.AsNoTracking()
            join userRole in dbContext.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
            join role in dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            select new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                Email = user.Email!,
                Role = role.Name!,
                user.IsActive
            };

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var role = query.Role.Trim();
            usersQuery = usersQuery.Where(user => user.Role == role);
        }

        var totalCount = await usersQuery.CountAsync(cancellationToken);
        var users = await usersQuery
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(user => new UserReportData(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.IsActive))
            .ToListAsync(cancellationToken);
        var userIds = users.Select(user => user.Id).ToArray();

        var tickets = await dbContext.Tickets.AsNoTracking()
            .Where(ticket => userIds.Contains(ticket.CreatedByUserId) &&
                ((ticket.CreatedAtUtc >= period.FromUtc && ticket.CreatedAtUtc <= period.ToUtc) ||
                    (ticket.ResolvedAtUtc >= period.FromUtc && ticket.ResolvedAtUtc <= period.ToUtc)))
            .Select(ticket => new EmployeeTicketData(
                ticket.CreatedByUserId,
                ticket.CreatedAtUtc,
                ticket.ResolvedAtUtc))
            .ToListAsync(cancellationToken);
        var comments = await dbContext.TicketComments.AsNoTracking()
            .Where(comment => userIds.Contains(comment.AuthorUserId) &&
                comment.CreatedAtUtc >= period.FromUtc &&
                comment.CreatedAtUtc <= period.ToUtc)
            .Select(comment => new UserActivityData(comment.AuthorUserId, comment.CreatedAtUtc, true))
            .ToListAsync(cancellationToken);
        var activity = await dbContext.UserActivityLogs.AsNoTracking()
            .Where(log => log.UserId.HasValue && userIds.Contains(log.UserId.Value) &&
                log.OccurredAtUtc >= period.FromUtc &&
                log.OccurredAtUtc <= period.ToUtc)
            .Select(log => new UserActivityData(log.UserId!.Value, log.OccurredAtUtc, log.Succeeded))
            .ToListAsync(cancellationToken);

        var items = users.Select(user =>
        {
            var userTickets = tickets.Where(ticket => ticket.UserId == user.Id).ToList();
            var userComments = comments.Where(comment => comment.UserId == user.Id).ToList();
            var userActivity = activity.Where(item => item.UserId == user.Id).ToList();
            var lastActivity = userTickets.Select(ticket => ticket.CreatedAtUtc)
                .Concat(userTickets.Where(ticket => ticket.ResolvedAtUtc.HasValue)
                    .Select(ticket => ticket.ResolvedAtUtc!.Value))
                .Concat(userComments.Select(comment => comment.OccurredAtUtc))
                .Concat(userActivity.Select(item => item.OccurredAtUtc))
                .Cast<DateTime?>()
                .Max();

            return new EmployeeActivityItemResponse(
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.IsActive,
                userTickets.Count(ticket => ticket.CreatedAtUtc >= period.FromUtc &&
                    ticket.CreatedAtUtc <= period.ToUtc),
                userTickets.Count(ticket => ticket.ResolvedAtUtc >= period.FromUtc &&
                    ticket.ResolvedAtUtc <= period.ToUtc),
                userComments.Count,
                userActivity.Count(item => item.Succeeded),
                userActivity.Count(item => !item.Succeeded),
                lastActivity);
        }).ToList();

        return new EmployeeActivityReportResponse(
            now,
            period,
            items,
            query.PageNumber,
            query.PageSize,
            totalCount,
            (int)Math.Ceiling(totalCount / (double)query.PageSize));
    }

    private async Task<IReadOnlyList<AgentPerformanceItemResponse>> BuildAgentPerformanceAsync(
        ReportingPeriodResponse period,
        CancellationToken cancellationToken)
    {
        var agents = await (
            from user in dbContext.Users.AsNoTracking()
            join userRole in dbContext.UserRoles.AsNoTracking() on user.Id equals userRole.UserId
            join role in dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where user.IsActive && role.Name == Roles.ITSupportSpecialist
            orderby user.FirstName, user.LastName
            select new AgentData(user.Id, user.FirstName, user.LastName, user.Email!))
            .ToListAsync(cancellationToken);
        var agentIds = agents.Select(agent => agent.Id).ToArray();
        var tickets = await dbContext.Tickets.AsNoTracking()
            .Where(ticket => ticket.AssignedToUserId.HasValue &&
                agentIds.Contains(ticket.AssignedToUserId.Value) &&
                !ticket.IsCancelled &&
                ((ticket.Status != TicketStatus.Resolved && ticket.Status != TicketStatus.Closed) ||
                    (ticket.ResolvedAtUtc >= period.FromUtc && ticket.ResolvedAtUtc <= period.ToUtc)))
            .Select(ticket => new AgentTicketData(
                ticket.AssignedToUserId!.Value,
                ticket.Status,
                ticket.Priority,
                ticket.CreatedAtUtc,
                ticket.ResolvedAtUtc))
            .ToListAsync(cancellationToken);

        return agents.Select(agent =>
        {
            var assigned = tickets.Where(ticket => ticket.AgentId == agent.Id).ToList();
            var resolved = assigned.Where(ticket =>
                ticket.ResolvedAtUtc >= period.FromUtc && ticket.ResolvedAtUtc <= period.ToUtc).ToList();
            var compliant = resolved.Count(ticket =>
                ResolutionHours(ticket.CreatedAtUtc, ticket.ResolvedAtUtc!.Value) <=
                    _options.GetSlaHours(ticket.Priority));

            return new AgentPerformanceItemResponse(
                agent.Id,
                agent.FirstName,
                agent.LastName,
                agent.Email,
                assigned.Count(ticket => ticket.Status is not TicketStatus.Resolved and not TicketStatus.Closed),
                assigned.Count(ticket => ticket.Status == TicketStatus.Pending),
                resolved.Count,
                AverageHours(resolved.Select(ticket =>
                    ticket.ResolvedAtUtc!.Value - ticket.CreatedAtUtc)),
                Percentage(compliant, resolved.Count));
        }).ToList();
    }

    private async Task<SlaReportResponse> BuildSlaReportAsync(
        ReportingPeriodResponse period,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var tickets = await dbContext.Tickets.AsNoTracking()
            .Where(ticket => !ticket.IsCancelled &&
                ticket.CreatedAtUtc >= period.FromUtc &&
                ticket.CreatedAtUtc <= period.ToUtc)
            .Select(ticket => new SlaTicketData(
                ticket.Priority,
                ticket.CreatedAtUtc,
                ticket.ResolvedAtUtc))
            .ToListAsync(cancellationToken);
        var metrics = Enum.GetValues<TicketPriority>().Select(priority =>
        {
            var target = _options.GetSlaHours(priority);
            var priorityTickets = tickets.Where(ticket => ticket.Priority == priority).ToList();
            var resolved = priorityTickets.Where(ticket => ticket.ResolvedAtUtc.HasValue).ToList();
            var active = priorityTickets.Where(ticket => !ticket.ResolvedAtUtc.HasValue).ToList();
            var compliant = resolved.Count(ticket =>
                ResolutionHours(ticket.CreatedAtUtc, ticket.ResolvedAtUtc!.Value) <= target);
            var breached = resolved.Count - compliant;
            var activeAtRisk = active.Count(ticket =>
            {
                var age = ResolutionHours(ticket.CreatedAtUtc, now);
                return age >= target * (_options.AtRiskThresholdPercentage / 100d) && age <= target;
            });
            var activeBreached = active.Count(ticket =>
                ResolutionHours(ticket.CreatedAtUtc, now) > target);

            return new SlaPriorityMetricResponse(
                priority,
                target,
                resolved.Count,
                compliant,
                breached,
                activeAtRisk,
                activeBreached,
                Percentage(compliant, resolved.Count),
                AverageHours(resolved.Select(ticket =>
                    ticket.ResolvedAtUtc!.Value - ticket.CreatedAtUtc)));
        }).ToList();
        var evaluated = metrics.Sum(metric => metric.EvaluatedTickets);
        var compliantTotal = metrics.Sum(metric => metric.CompliantTickets);

        return new SlaReportResponse(
            now,
            period,
            new SlaSummaryResponse(
                evaluated,
                compliantTotal,
                metrics.Sum(metric => metric.BreachedTickets),
                metrics.Sum(metric => metric.ActiveAtRiskTickets),
                metrics.Sum(metric => metric.ActiveBreachedTickets),
                Percentage(compliantTotal, evaluated)),
            metrics);
    }

    private ReportingPeriodResponse ResolvePeriod(
        DateTime? fromUtc,
        DateTime? toUtc,
        DateTime now)
    {
        var to = NormalizeUtc(toUtc ?? now);
        var from = NormalizeUtc(fromUtc ?? to.AddDays(-_options.DefaultPeriodDays));
        if (from > to)
            throw new ArgumentException("FromUtc must be earlier than or equal to ToUtc.");
        return new ReportingPeriodResponse(from, to);
    }

    private void EnsureCanViewReports()
    {
        if (!currentUser.HasPermission(Permission.ViewTicketReports))
            throw new ForbiddenAccessException(
                "Only administrators and managers can access reports.");
    }

    private static int GetCount(
        IReadOnlyDictionary<TicketStatus, int> counts,
        TicketStatus status) => counts.GetValueOrDefault(status);

    private static DateTime NormalizeUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static double ResolutionHours(DateTime createdAtUtc, DateTime resolvedAtUtc) =>
        Math.Max(0, (resolvedAtUtc - createdAtUtc).TotalHours);

    private static double? AverageHours(IEnumerable<TimeSpan> durations)
    {
        var values = durations.Select(duration => Math.Max(0, duration.TotalHours)).ToList();
        return values.Count == 0 ? null : Math.Round(values.Average(), 2);
    }

    private static double? Percentage(int numerator, int denominator) =>
        denominator == 0 ? null : Math.Round(numerator * 100d / denominator, 2);

    private sealed record MonthlyTicketData(
        DateTime CreatedAtUtc,
        DateTime? ResolvedAtUtc,
        DateTime? ClosedAtUtc,
        bool IsCancelled);

    private sealed record UserReportData(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string Role,
        bool IsActive);

    private sealed record EmployeeTicketData(
        Guid UserId,
        DateTime CreatedAtUtc,
        DateTime? ResolvedAtUtc);

    private sealed record UserActivityData(
        Guid UserId,
        DateTime OccurredAtUtc,
        bool Succeeded);

    private sealed record AgentData(
        Guid Id,
        string FirstName,
        string LastName,
        string Email);

    private sealed record AgentTicketData(
        Guid AgentId,
        TicketStatus Status,
        TicketPriority Priority,
        DateTime CreatedAtUtc,
        DateTime? ResolvedAtUtc);

    private sealed record SlaTicketData(
        TicketPriority Priority,
        DateTime CreatedAtUtc,
        DateTime? ResolvedAtUtc);
}
