using HelpDesk.Domain;

namespace HelpDesk.Application.Tests.Attachments;

public sealed class TicketAttachmentDomainTests
{
    private static readonly DateTime CreatedAtUtc =
        new(2026, 8, 8, 8, 0, 0, DateTimeKind.Utc);

    [Test]
    public void AddAttachment_AddsMetadataAndAuditHistory()
    {
        var creatorId = Guid.NewGuid();
        var ticket = CreateTicket(creatorId);

        var attachment = ticket.AddAttachment(
            creatorId,
            "network-log.txt",
            "2026/08/random.txt",
            "text/plain",
            ".txt",
            128,
            new string('A', 64),
            CreatedAtUtc.AddMinutes(5));

        Assert.Multiple(() =>
        {
            Assert.That(ticket.Attachments, Has.Count.EqualTo(1));
            Assert.That(attachment.TicketId, Is.EqualTo(ticket.Id));
            Assert.That(attachment.UploadedByUserId, Is.EqualTo(creatorId));
            Assert.That(attachment.OriginalFileName, Is.EqualTo("network-log.txt"));
            Assert.That(attachment.SizeBytes, Is.EqualTo(128));
            Assert.That(ticket.History.Last().Action, Is.EqualTo("Attachment uploaded"));
            Assert.That(ticket.UpdatedAtUtc, Is.EqualTo(CreatedAtUtc.AddMinutes(5)));
        });
    }

    [Test]
    public void ClosedTicket_RejectsNewAttachment()
    {
        var ticket = CreateTicket(Guid.NewGuid());
        ticket.ChangeStatus(Guid.NewGuid(), TicketStatus.Closed, CreatedAtUtc.AddMinutes(1));

        Assert.Throws<DomainRuleException>(() => ticket.AddAttachment(
            Guid.NewGuid(),
            "evidence.pdf",
            "2026/08/random.pdf",
            "application/pdf",
            ".pdf",
            128,
            new string('B', 64),
            CreatedAtUtc.AddMinutes(2)));
    }

    private static Ticket CreateTicket(Guid creatorId) =>
        Ticket.Create(
            creatorId,
            "HD-ATTACHMENT-TEST",
            "Attachment domain test",
            "Verifies attachment invariants.",
            TicketCategory.Network,
            TicketPriority.Medium,
            CreatedAtUtc);
}
