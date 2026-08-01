using HelpDesk.Domain;
using HelpDesk.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<TicketHistory> TicketHistories => Set<TicketHistory>();

    public DbSet<TicketAssignmentHistory> TicketAssignmentHistories => Set<TicketAssignmentHistory>();

    public DbSet<TicketInternalNote> TicketInternalNotes => Set<TicketInternalNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(token => token.Id);
            entity.Property(token => token.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(token => token.TokenHash).IsUnique();
            entity.HasIndex(token => token.UserId);
        });

        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.HasKey(log => log.Id);
            entity.Property(log => log.Action).HasMaxLength(100).IsRequired();
            entity.Property(log => log.Resource).HasMaxLength(100).IsRequired();
            entity.Property(log => log.ResourceId).HasMaxLength(100);
            entity.Property(log => log.UserEmail).HasMaxLength(256);
            entity.Property(log => log.IpAddress).HasMaxLength(64);
            entity.HasIndex(log => log.OccurredAtUtc);
            entity.HasIndex(log => log.UserId);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(ticket => ticket.Id);
            entity.Property(ticket => ticket.ReferenceNumber).HasMaxLength(32).IsRequired();
            entity.HasIndex(ticket => ticket.ReferenceNumber).IsUnique();
            entity.Property(ticket => ticket.Title).HasMaxLength(200).IsRequired();
            entity.Property(ticket => ticket.Description).HasMaxLength(4000).IsRequired();
            entity.HasIndex(ticket => ticket.CreatedByUserId);
            entity.HasIndex(ticket => new { ticket.Status, ticket.Priority });
            entity.Property(ticket => ticket.RowVersion).IsRowVersion();
            entity.HasMany(ticket => ticket.History)
                .WithOne()
                .HasForeignKey(history => history.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(ticket => ticket.AssignmentHistory)
                .WithOne()
                .HasForeignKey(history => history.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(ticket => ticket.InternalNotes)
                .WithOne()
                .HasForeignKey(note => note.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(ticket => ticket.History).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(ticket => ticket.AssignmentHistory).UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(ticket => ticket.InternalNotes).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<TicketHistory>(entity =>
        {
            entity.HasKey(history => history.Id);
            entity.Property(history => history.Action).HasMaxLength(100).IsRequired();
            entity.Property(history => history.PreviousValue).HasMaxLength(500);
            entity.Property(history => history.NewValue).HasMaxLength(500);
            entity.HasIndex(history => history.TicketId);
        });

        modelBuilder.Entity<TicketAssignmentHistory>(entity =>
        {
            entity.HasKey(history => history.Id);
            entity.HasIndex(history => history.TicketId);
            entity.HasIndex(history => history.AssignedAgentId);
        });

        modelBuilder.Entity<TicketInternalNote>(entity =>
        {
            entity.HasKey(note => note.Id);
            entity.Property(note => note.Content).HasMaxLength(4000).IsRequired();
            entity.HasIndex(note => note.TicketId);
        });
    }
}
