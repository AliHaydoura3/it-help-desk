using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Application.Common.Authorization;
using HelpDesk.Domain;

namespace HelpDesk.Application.Tests.Authorization;

public sealed class TicketAccessPolicyTests
{
    private static readonly DateTime OccurredAtUtc = new(2026, 8, 7, 0, 0, 0, DateTimeKind.Utc);

    [Test]
    public void SupportAgent_CanChangeStatusOnlyAfterAssignmentToSelf()
    {
        var creatorId = Guid.NewGuid();
        var assignedAgentId = Guid.NewGuid();
        var otherAgentId = Guid.NewGuid();
        var ticket = CreateTicket(creatorId);
        ticket.Assign(Guid.NewGuid(), assignedAgentId, TicketAssignmentType.Manual, OccurredAtUtc);

        Assert.Multiple(() =>
        {
            Assert.That(User(assignedAgentId, Roles.ITSupportSpecialist).CanChangeTicketStatus(ticket), Is.True);
            Assert.That(User(otherAgentId, Roles.ITSupportSpecialist).CanChangeTicketStatus(ticket), Is.False);
            Assert.That(User(Guid.NewGuid(), Roles.Admin).CanChangeTicketStatus(ticket), Is.True);
            Assert.That(User(Guid.NewGuid(), Roles.Manager).CanChangeTicketStatus(ticket), Is.False);
            Assert.That(User(creatorId, Roles.Employee).CanChangeTicketStatus(ticket), Is.False);
        });
    }

    [Test]
    public void Manager_CanReadOtherTicketsButCannotMutateOrCommentOnThem()
    {
        var ticket = CreateTicket(Guid.NewGuid());
        var manager = User(Guid.NewGuid(), Roles.Manager);

        Assert.Multiple(() =>
        {
            Assert.That(manager.CanReadTicket(ticket), Is.True);
            Assert.That(manager.CanEditTicket(ticket), Is.False);
            Assert.That(manager.CanCancelTicket(ticket), Is.False);
            Assert.That(manager.CanChangeTicketStatus(ticket), Is.False);
            Assert.That(manager.CanCommentOnTicket(ticket), Is.False);
            Assert.That(manager.CanUploadAttachment(ticket), Is.False);
        });
    }

    [Test]
    public void Manager_RetainsEmployeeCapabilitiesForOwnOpenTicket()
    {
        var managerId = Guid.NewGuid();
        var ticket = CreateTicket(managerId);
        var manager = User(managerId, Roles.Manager);

        Assert.Multiple(() =>
        {
            Assert.That(manager.CanReadTicket(ticket), Is.True);
            Assert.That(manager.CanEditTicket(ticket), Is.True);
            Assert.That(manager.CanCancelTicket(ticket), Is.True);
            Assert.That(manager.CanCommentOnTicket(ticket), Is.True);
            Assert.That(manager.CanUploadAttachment(ticket), Is.True);
        });
    }

    [Test]
    public void SupportAgent_CanTriageAndCommentAcrossTheQueue()
    {
        var ticket = CreateTicket(Guid.NewGuid());
        var supportAgent = User(Guid.NewGuid(), Roles.ITSupportSpecialist);

        Assert.Multiple(() =>
        {
            Assert.That(supportAgent.CanReadTicket(ticket), Is.True);
            Assert.That(supportAgent.CanEditTicket(ticket), Is.True);
            Assert.That(supportAgent.CanCancelTicket(ticket), Is.True);
            Assert.That(supportAgent.CanCommentOnTicket(ticket), Is.True);
            Assert.That(supportAgent.CanUploadAttachment(ticket), Is.True);
            Assert.That(supportAgent.CanChangeTicketStatus(ticket), Is.False);
        });
    }

    [Test]
    public void ClosedOrCancelledTickets_RejectNewCommentsForEveryRole()
    {
        var closedTicket = CreateTicket(Guid.NewGuid());
        closedTicket.ChangeStatus(Guid.NewGuid(), TicketStatus.Closed, OccurredAtUtc);
        var cancelledTicket = CreateTicket(Guid.NewGuid());
        cancelledTicket.Cancel(Guid.NewGuid(), OccurredAtUtc);
        var admin = User(Guid.NewGuid(), Roles.Admin);

        Assert.Multiple(() =>
        {
            Assert.That(admin.CanCommentOnTicket(closedTicket), Is.False);
            Assert.That(admin.CanCommentOnTicket(cancelledTicket), Is.False);
            Assert.That(admin.CanUploadAttachment(closedTicket), Is.False);
            Assert.That(admin.CanUploadAttachment(cancelledTicket), Is.False);
        });
    }

    private static Ticket CreateTicket(Guid creatorId) =>
        Ticket.Create(
            creatorId,
            "HD-20260807-TEST",
            "Authorization test",
            "Verifies ticket policy behavior.",
            TicketCategory.Software,
            TicketPriority.Medium,
            OccurredAtUtc);

    private static ICurrentUser User(Guid userId, string role) =>
        new TestCurrentUser(userId, role);

    private sealed class TestCurrentUser(Guid userId, string role) : ICurrentUser
    {
        public Guid UserId { get; } = userId;
        public string? Email => "authorization-test@example.com";
        public bool IsInRole(string expectedRole) =>
            role.Equals(expectedRole, StringComparison.OrdinalIgnoreCase);
    }
}
