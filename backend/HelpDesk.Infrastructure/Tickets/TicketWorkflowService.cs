using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Application.Abstractions.Tickets;
using HelpDesk.Application.Common.Authorization;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Features.Tickets;
using HelpDesk.Application.Features.Tickets.Workflow;
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Identity;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Tickets;

public sealed class TicketWorkflowService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser) : ITicketWorkflowService
{
    public async Task<TicketResponse> AssignAsync(
        AssignTicketCommand command,
        CancellationToken cancellationToken)
    {
        EnsureCanManageWorkflow();
        await EnsureIsActiveSupportAgentAsync(command.AgentUserId, cancellationToken);
        var ticket = await FindTicketAsync(command.TicketId, cancellationToken);
        var historyCount = ticket.History.Count;
        var assignmentCount = ticket.AssignmentHistory.Count;
        ticket.Assign(currentUser.UserId, command.AgentUserId, TicketAssignmentType.Manual, DateTime.UtcNow);
        TrackNewWorkflowEntries(ticket, historyCount, assignmentCount);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TicketMappings.ToResponse(ticket);
    }

    public async Task<TicketResponse> AutoAssignAsync(
        AutoAssignTicketCommand command,
        CancellationToken cancellationToken)
    {
        EnsureCanManageWorkflow();
        var ticket = await FindTicketAsync(command.TicketId, cancellationToken);
        var agentId = await BuildAssignableAgentDataQuery()
            .OrderBy(agent => agent.ActiveTicketCount)
            .ThenBy(agent => agent.FirstName)
            .ThenBy(agent => agent.LastName)
            .Select(agent => (Guid?)agent.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("No active support agents are available for assignment.");
        var historyCount = ticket.History.Count;
        var assignmentCount = ticket.AssignmentHistory.Count;
        ticket.Assign(currentUser.UserId, agentId, TicketAssignmentType.Automatic, DateTime.UtcNow);
        TrackNewWorkflowEntries(ticket, historyCount, assignmentCount);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TicketMappings.ToResponse(ticket);
    }

    public async Task<TicketResponse> EscalateAsync(
        EscalateTicketCommand command,
        CancellationToken cancellationToken)
    {
        EnsureCanManageWorkflow();
        var ticket = await FindTicketAsync(command.TicketId, cancellationToken);
        var historyCount = ticket.History.Count;
        ticket.Escalate(currentUser.UserId, command.Level, command.Reason, DateTime.UtcNow);
        dbContext.TicketHistories.AddRange(ticket.History.Skip(historyCount));
        await dbContext.SaveChangesAsync(cancellationToken);
        return TicketMappings.ToResponse(ticket);
    }

    public async Task<InternalNoteResponse> AddInternalNoteAsync(
        AddInternalNoteCommand command,
        CancellationToken cancellationToken)
    {
        EnsureCanUseInternalNotes();
        var ticket = await FindTicketAsync(command.TicketId, cancellationToken);
        var historyCount = ticket.History.Count;
        var note = ticket.AddInternalNote(currentUser.UserId, command.Content, DateTime.UtcNow);
        dbContext.TicketHistories.AddRange(ticket.History.Skip(historyCount));
        dbContext.TicketInternalNotes.Add(note);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapNote(note);
    }

    public async Task<IReadOnlyList<InternalNoteResponse>> GetInternalNotesAsync(
        GetInternalNotesQuery query,
        CancellationToken cancellationToken)
    {
        EnsureCanUseInternalNotes();
        await EnsureTicketExistsAsync(query.TicketId, cancellationToken);
        return await dbContext.TicketInternalNotes.AsNoTracking()
            .Where(note => note.TicketId == query.TicketId)
            .OrderByDescending(note => note.CreatedAtUtc)
            .Select(note => new InternalNoteResponse(note.Id, note.AuthorUserId, note.Content, note.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssignmentHistoryResponse>> GetAssignmentHistoryAsync(
        GetAssignmentHistoryQuery query,
        CancellationToken cancellationToken)
    {
        EnsureCanMonitorWorkflow();
        await EnsureTicketExistsAsync(query.TicketId, cancellationToken);
        return await dbContext.TicketAssignmentHistories.AsNoTracking()
            .Where(history => history.TicketId == query.TicketId)
            .OrderByDescending(history => history.OccurredAtUtc)
            .Select(history => new AssignmentHistoryResponse(history.PreviousAgentId,
                history.AssignedAgentId, history.ActorUserId, history.AssignmentType, history.OccurredAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssignableAgentResponse>> GetAssignableAgentsAsync(
        GetAssignableAgentsQuery query,
        CancellationToken cancellationToken)
    {
        EnsureCanManageWorkflow();
        var agents = await BuildAssignableAgentDataQuery()
            .OrderBy(agent => agent.ActiveTicketCount)
            .ThenBy(agent => agent.FirstName)
            .ThenBy(agent => agent.LastName)
            .ToListAsync(cancellationToken);

        return agents
            .Select(agent => new AssignableAgentResponse(
                agent.Id,
                agent.FirstName,
                agent.LastName,
                agent.Email,
                agent.ActiveTicketCount))
            .ToList();
    }

    private IQueryable<ApplicationUser> BuildAssignableUsersQuery()
    {
        var supportRoleId = dbContext.Roles
            .Where(role => role.NormalizedName == Roles.ITSupportSpecialist.ToUpperInvariant())
            .Select(role => role.Id);
        var supportUserIds = dbContext.UserRoles
            .Where(userRole => supportRoleId.Contains(userRole.RoleId))
            .Select(userRole => userRole.UserId);

        return dbContext.Users.AsNoTracking()
            .Where(user => user.IsActive && supportUserIds.Contains(user.Id));
    }

    private IQueryable<AssignableAgentData> BuildAssignableAgentDataQuery()
    {
        return BuildAssignableUsersQuery()
            .Select(user => new AssignableAgentData
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                ActiveTicketCount = dbContext.Tickets.Count(ticket =>
                    ticket.AssignedToUserId == user.Id &&
                    !ticket.IsCancelled &&
                    ticket.Status != TicketStatus.Resolved &&
                    ticket.Status != TicketStatus.Closed)
            });
    }

    private async Task EnsureIsActiveSupportAgentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var isAgent = await BuildAssignableUsersQuery().AnyAsync(agent => agent.Id == userId, cancellationToken);
        if (!isAgent) throw new InvalidOperationException("The selected user is not an active support agent.");
    }

    private async Task<Ticket> FindTicketAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Tickets.FirstOrDefaultAsync(ticket => ticket.Id == id, cancellationToken)
        ?? throw new KeyNotFoundException($"Ticket '{id}' does not exist.");

    private async Task EnsureTicketExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!await dbContext.Tickets.AnyAsync(ticket => ticket.Id == id, cancellationToken))
            throw new KeyNotFoundException($"Ticket '{id}' does not exist.");
    }

    private bool CanManage => currentUser.IsInRole(Roles.Admin) || currentUser.IsInRole(Roles.ITSupportSpecialist);
    private void EnsureCanManageWorkflow() { if (!CanManage) throw new ForbiddenAccessException("Only administrators and support agents can manage ticket workflow."); }
    private void EnsureCanMonitorWorkflow() { if (!CanManage && !currentUser.IsInRole(Roles.Manager)) throw new ForbiddenAccessException("You cannot view assignment history."); }
    private void EnsureCanUseInternalNotes() { if (!CanManage) throw new ForbiddenAccessException("Internal notes are restricted to administrators and support agents."); }
    private static InternalNoteResponse MapNote(TicketInternalNote note) => new(note.Id, note.AuthorUserId, note.Content, note.CreatedAtUtc);
    private void TrackNewWorkflowEntries(Ticket ticket, int previousHistoryCount, int previousAssignmentCount)
    {
        dbContext.TicketHistories.AddRange(ticket.History.Skip(previousHistoryCount));
        dbContext.TicketAssignmentHistories.AddRange(
            ticket.AssignmentHistory.Skip(previousAssignmentCount));
    }

    private sealed class AssignableAgentData
    {
        public Guid Id { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Email { get; init; }
        public int ActiveTicketCount { get; init; }
    }
}

internal static class TicketMappings
{
    internal static TicketResponse ToResponse(Ticket ticket) => new(
        ticket.Id, ticket.ReferenceNumber, ticket.Title, ticket.Description, ticket.Category,
        ticket.Priority, ticket.Status, ticket.EscalationLevel, ticket.IsCancelled,
        ticket.CreatedByUserId, ticket.AssignedToUserId, ticket.CreatedAtUtc, ticket.UpdatedAtUtc);
}
