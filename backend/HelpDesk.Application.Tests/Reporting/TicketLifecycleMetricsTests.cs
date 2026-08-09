using HelpDesk.Domain;

namespace HelpDesk.Application.Tests.Reporting;

public sealed class TicketLifecycleMetricsTests
{
    [Test]
    public void ResolvingAndClosingTicket_CapturesFirstResolutionAndClosureTimes()
    {
        var creatorId = Guid.NewGuid();
        var createdAt = new DateTime(2026, 8, 7, 8, 0, 0, DateTimeKind.Utc);
        var resolvedAt = createdAt.AddHours(3);
        var closedAt = resolvedAt.AddHours(2);
        var ticket = Ticket.Create(
            creatorId,
            "HD-REPORT-1",
            "Reporting lifecycle",
            "Capture resolution metrics.",
            TicketCategory.Hardware,
            TicketPriority.High,
            createdAt);

        ticket.ChangeStatus(creatorId, TicketStatus.Resolved, resolvedAt);
        ticket.ChangeStatus(creatorId, TicketStatus.Closed, closedAt);

        Assert.Multiple(() =>
        {
            Assert.That(ticket.ResolvedAtUtc, Is.EqualTo(resolvedAt));
            Assert.That(ticket.ClosedAtUtc, Is.EqualTo(closedAt));
        });
    }

    [Test]
    public void ClosingDirectly_AlsoCapturesResolutionTime()
    {
        var createdAt = new DateTime(2026, 8, 7, 8, 0, 0, DateTimeKind.Utc);
        var closedAt = createdAt.AddHours(1);
        var ticket = Ticket.Create(
            Guid.NewGuid(),
            "HD-REPORT-2",
            "Direct closure",
            "Close without resolved state.",
            TicketCategory.AccessRequest,
            TicketPriority.Low,
            createdAt);

        ticket.ChangeStatus(Guid.NewGuid(), TicketStatus.Closed, closedAt);

        Assert.Multiple(() =>
        {
            Assert.That(ticket.ResolvedAtUtc, Is.EqualTo(closedAt));
            Assert.That(ticket.ClosedAtUtc, Is.EqualTo(closedAt));
        });
    }
}
