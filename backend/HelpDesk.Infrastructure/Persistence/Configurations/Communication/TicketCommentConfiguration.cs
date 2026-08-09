using HelpDesk.Domain;
using HelpDesk.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations.Communication;

public sealed class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.HasKey(comment => comment.Id);
        builder.Property(comment => comment.Content).HasMaxLength(4000).IsRequired();
        builder.HasIndex(comment => new { comment.TicketId, comment.CreatedAtUtc });
        builder.HasIndex(comment => comment.ParentCommentId);

        builder.HasOne<Ticket>()
            .WithMany()
            .HasForeignKey(comment => comment.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(comment => comment.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TicketComment>()
            .WithMany()
            .HasForeignKey(comment => comment.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(comment => comment.Mentions)
            .WithOne()
            .HasForeignKey(mention => mention.TicketCommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(comment => comment.Mentions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
