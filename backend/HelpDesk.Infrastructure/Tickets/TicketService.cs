using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Application.Abstractions.Tickets;
using HelpDesk.Application.Abstractions.Communication;
using HelpDesk.Application.Common.Authorization;
using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Features.Tickets;
using HelpDesk.Application.Features.Tickets.CancelTicket;
using HelpDesk.Application.Features.Tickets.ChangeTicketStatus;
using HelpDesk.Application.Features.Tickets.CreateTicket;
using HelpDesk.Application.Features.Tickets.GetTicketById;
using HelpDesk.Application.Features.Tickets.GetTicketHistory;
using HelpDesk.Application.Features.Tickets.GetTicketSummary;
using HelpDesk.Application.Features.Tickets.GetTickets;
using HelpDesk.Application.Features.Tickets.UpdateTicket;
using HelpDesk.Application.Common.Notifications;
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Application.Abstractions.Admin;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Tickets;

public sealed class TicketService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    INotificationQueue notificationQueue,
    IOperationalSettingsReader settingsReader,
    ITicketCategoryCatalog categoryCatalog) : ITicketService
{
    public async Task<TicketResponse> CreateAsync(CreateTicketCommand command, CancellationToken cancellationToken)
    {
        await categoryCatalog.EnsureActiveAsync(command.Category, cancellationToken);
        if (currentUser.IsInRole(Roles.Employee))
        {
            var settings = await settingsReader.GetAsync(cancellationToken);
            var activeTicketCount = await dbContext.Tickets.CountAsync(ticket =>
                ticket.CreatedByUserId == currentUser.UserId &&
                !ticket.IsCancelled &&
                ticket.Status != TicketStatus.Resolved &&
                ticket.Status != TicketStatus.Closed, cancellationToken);
            if (activeTicketCount >= settings.MaximumOpenTicketsPerEmployee)
                throw new InvalidOperationException(
                    $"You have reached the limit of {settings.MaximumOpenTicketsPerEmployee} open tickets.");
        }

        var now = DateTime.UtcNow;
        var ticket = Ticket.Create(currentUser.UserId, GenerateReference(now), command.Title,
            command.Description, command.Category, command.Priority, now);
        dbContext.Tickets.Add(ticket);
        await notificationQueue.QueueToRolesAsync(
            new NotificationMessage(
                currentUser.UserId,
                ticket.Id,
                NotificationType.TicketCreated,
                "New support ticket",
                $"Ticket {ticket.ReferenceNumber} was created with {ticket.Priority} priority.",
                []),
            [Roles.Admin, Roles.ITSupportSpecialist],
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TicketMappings.ToResponse(ticket);
    }

    public async Task<GetTicketsResponse> GetAllAsync(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Tickets.AsNoTracking().Where(ticket => !ticket.IsCancelled);
        if (!CanMonitorAll) query = query.Where(ticket => ticket.CreatedByUserId == currentUser.UserId);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(ticket => ticket.ReferenceNumber.Contains(search) || ticket.Title.Contains(search) || ticket.Description.Contains(search));
        }
        if (request.Category.HasValue) query = query.Where(ticket => ticket.Category == request.Category);
        if (request.Priority.HasValue) query = query.Where(ticket => ticket.Priority == request.Priority);
        if (request.Status.HasValue) query = query.Where(ticket => ticket.Status == request.Status);

        var totalCount = await query.CountAsync(cancellationToken);
        var tickets = await query.OrderByDescending(ticket => ticket.CreatedAtUtc)
            .ThenByDescending(ticket => ticket.Id)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return new GetTicketsResponse(tickets.Select(TicketMappings.ToResponse).ToList(), request.PageNumber, request.PageSize,
            totalCount, (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }

    public async Task<TicketResponse> GetByIdAsync(GetTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await FindAsync(request.Id, cancellationToken); EnsureCanRead(ticket); return TicketMappings.ToResponse(ticket);
    }

    public async Task<TicketResponse> UpdateAsync(UpdateTicketCommand command, CancellationToken cancellationToken)
    {
        var ticket = await FindAsync(command.Id, cancellationToken); EnsureCanEdit(ticket);
        if (ticket.Category != command.Category)
            await categoryCatalog.EnsureActiveAsync(command.Category, cancellationToken);
        var historyCount = ticket.History.Count;
        ticket.UpdateDetails(currentUser.UserId, command.Title, command.Description,
            command.Category, command.Priority, DateTime.UtcNow);
        TrackNewHistory(ticket, historyCount);
        await QueueTicketAlertAsync(
            ticket,
            NotificationType.TicketUpdated,
            "Ticket updated",
            $"Ticket {ticket.ReferenceNumber} was updated.",
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken); return TicketMappings.ToResponse(ticket);
    }

    public async Task<TicketResponse> ChangeStatusAsync(ChangeTicketStatusCommand command, CancellationToken cancellationToken)
    {
        var ticket = await FindAsync(command.Id, cancellationToken);
        EnsureCanChangeStatus(ticket);
        var historyCount = ticket.History.Count;
        var previousStatus = ticket.Status;
        ticket.ChangeStatus(currentUser.UserId, command.Status, DateTime.UtcNow);
        TrackNewHistory(ticket, historyCount);
        if (previousStatus != ticket.Status)
        {
            await QueueTicketAlertAsync(
                ticket,
                NotificationType.TicketStatusChanged,
                "Ticket status changed",
                $"Ticket {ticket.ReferenceNumber} changed from {previousStatus} to {ticket.Status}.",
                cancellationToken);
        }
        await dbContext.SaveChangesAsync(cancellationToken); return TicketMappings.ToResponse(ticket);
    }

    public async Task CancelAsync(CancelTicketCommand command, CancellationToken cancellationToken)
    {
        var ticket = await FindAsync(command.Id, cancellationToken); EnsureCanCancel(ticket);
        var historyCount = ticket.History.Count;
        ticket.Cancel(currentUser.UserId, DateTime.UtcNow);
        TrackNewHistory(ticket, historyCount);
        await QueueTicketAlertAsync(
            ticket,
            NotificationType.TicketCancelled,
            "Ticket cancelled",
            $"Ticket {ticket.ReferenceNumber} was cancelled.",
            cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TicketHistoryResponse>> GetHistoryAsync(GetTicketHistoryQuery request, CancellationToken cancellationToken)
    {
        var ticket = await FindAsync(request.Id, cancellationToken); EnsureCanRead(ticket);
        return await dbContext.TicketHistories.AsNoTracking().Where(history => history.TicketId == request.Id)
            .OrderByDescending(history => history.OccurredAtUtc)
            .Select(history => new TicketHistoryResponse(history.Action, history.PreviousValue, history.NewValue, history.ActorUserId, history.OccurredAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<TicketSummaryResponse> GetSummaryAsync(GetTicketSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.HasPermission(Permission.ViewTicketReports))
            throw new ForbiddenAccessException("Only administrators and managers can access ticket reports.");
        var tickets = dbContext.Tickets.AsNoTracking().Where(ticket => !ticket.IsCancelled);
        return new TicketSummaryResponse(
            await tickets.CountAsync(cancellationToken),
            await tickets.CountAsync(ticket => ticket.Status == TicketStatus.Open, cancellationToken),
            await tickets.CountAsync(ticket => ticket.Status == TicketStatus.InProgress, cancellationToken),
            await tickets.CountAsync(ticket => ticket.Status == TicketStatus.Pending, cancellationToken),
            await tickets.CountAsync(ticket => ticket.Status == TicketStatus.Resolved, cancellationToken),
            await tickets.CountAsync(ticket => ticket.Status == TicketStatus.Closed, cancellationToken),
            await tickets.CountAsync(ticket => ticket.Priority == TicketPriority.Critical, cancellationToken));
    }

    private bool CanMonitorAll => currentUser.HasPermission(Permission.MonitorAllTickets);
    private void EnsureCanRead(Ticket ticket)
    {
        if (!currentUser.CanReadTicket(ticket))
            throw new ForbiddenAccessException("You cannot access this ticket.");
    }

    private void EnsureCanEdit(Ticket ticket)
    {
        if (ticket.IsCancelled)
            throw new InvalidOperationException("A cancelled ticket cannot be modified.");

        if (!currentUser.CanEditTicket(ticket))
            throw new ForbiddenAccessException("You cannot modify this ticket.");
    }

    private void EnsureCanCancel(Ticket ticket)
    {
        if (ticket.IsCancelled)
            throw new InvalidOperationException("This ticket is already cancelled.");

        if (!currentUser.CanCancelTicket(ticket))
            throw new ForbiddenAccessException("You cannot cancel this ticket.");
    }

    private void EnsureCanChangeStatus(Ticket ticket)
    {
        if (ticket.IsCancelled)
            throw new InvalidOperationException(
                "A cancelled ticket cannot have its status changed.");

        if (currentUser.CanChangeTicketStatus(ticket)) return;

        if (currentUser.HasPermission(Permission.ChangeAssignedTicketStatus))
        {
            throw new ForbiddenAccessException(ticket.AssignedToUserId.HasValue
                ? "Only the assigned support agent or an administrator can change this ticket's status."
                : "Assign this ticket to a support agent before changing its status.");
        }

        throw new ForbiddenAccessException(
            "Only the assigned support agent or an administrator can change ticket status.");
    }
    private async Task<Ticket> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Tickets.FirstOrDefaultAsync(ticket => ticket.Id == id, cancellationToken)
        ?? throw new KeyNotFoundException($"Ticket '{id}' does not exist.");
    private static string GenerateReference(DateTime now) => $"HD-{now:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
    private void TrackNewHistory(Ticket ticket, int previousCount)
    {
        dbContext.TicketHistories.AddRange(ticket.History.Skip(previousCount));
    }

    private Task QueueTicketAlertAsync(
        Ticket ticket,
        NotificationType type,
        string title,
        string message,
        CancellationToken cancellationToken)
    {
        var recipientIds = new List<Guid> { ticket.CreatedByUserId };
        if (ticket.AssignedToUserId.HasValue)
            recipientIds.Add(ticket.AssignedToUserId.Value);

        return notificationQueue.QueueAsync(
            new NotificationMessage(
                currentUser.UserId,
                ticket.Id,
                type,
                title,
                message,
                recipientIds),
            cancellationToken);
    }
}
