using HelpDesk.Domain;
using HelpDesk.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations.Attachments;

public sealed class TicketAttachmentConfiguration
    : IEntityTypeConfiguration<TicketAttachment>
{
    public void Configure(EntityTypeBuilder<TicketAttachment> builder)
    {
        builder.HasKey(attachment => attachment.Id);
        builder.Property(attachment => attachment.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(attachment => attachment.StorageKey)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(attachment => attachment.ContentType)
            .HasMaxLength(127)
            .IsRequired();
        builder.Property(attachment => attachment.Extension)
            .HasMaxLength(16)
            .IsRequired();
        builder.Property(attachment => attachment.Sha256Hash)
            .HasMaxLength(64)
            .IsFixedLength()
            .IsRequired();
        builder.HasIndex(attachment => attachment.StorageKey).IsUnique();
        builder.HasIndex(attachment => new
        {
            attachment.TicketId,
            attachment.UploadedAtUtc
        });

        builder.HasOne<Ticket>()
            .WithMany(ticket => ticket.Attachments)
            .HasForeignKey(attachment => attachment.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(attachment => attachment.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
