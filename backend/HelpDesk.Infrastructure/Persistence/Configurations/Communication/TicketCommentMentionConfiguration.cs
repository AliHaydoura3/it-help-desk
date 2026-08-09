using HelpDesk.Domain;
using HelpDesk.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations.Communication;

public sealed class TicketCommentMentionConfiguration : IEntityTypeConfiguration<TicketCommentMention>
{
    public void Configure(EntityTypeBuilder<TicketCommentMention> builder)
    {
        builder.HasKey(mention => new { mention.TicketCommentId, mention.MentionedUserId });
        builder.HasIndex(mention => mention.MentionedUserId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(mention => mention.MentionedUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
