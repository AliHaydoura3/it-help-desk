using HelpDesk.Domain;
using HelpDesk.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations.Communication;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(notification => notification.Id);
        builder.Property(notification => notification.Title).HasMaxLength(200).IsRequired();
        builder.Property(notification => notification.Message).HasMaxLength(1000).IsRequired();
        builder.Property(notification => notification.LastEmailError).HasMaxLength(1000);
        builder.Property(notification => notification.LastRealtimeError).HasMaxLength(1000);
        builder.HasIndex(notification => new
        {
            notification.RecipientUserId,
            notification.IsRead,
            notification.CreatedAtUtc
        });
        builder.HasIndex(notification => new
        {
            notification.EmailStatus,
            notification.EmailAttempts
        });
        builder.HasIndex(notification => new
        {
            notification.RealtimeDeliveredAtUtc,
            notification.RealtimeAttempts
        });
        builder.HasIndex(notification => notification.TicketId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(notification => notification.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(notification => notification.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Ticket>()
            .WithMany()
            .HasForeignKey(notification => notification.TicketId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
