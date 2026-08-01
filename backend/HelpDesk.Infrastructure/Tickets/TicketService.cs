using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Application.Abstractions.Tickets;
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
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Tickets;

public sealed class TicketService(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser) : ITicketService
{
    public async Task<TicketResponse> CreateAsync(CreateTicketCommand command, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var ticket = Ticket.Create(currentUser.UserId, GenerateReference(now), command.Title,
            command.Description, command.Category, command.Priority, now);
        dbContext.Tickets.Add(ticket);
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
        var historyCount = ticket.History.Count;
        ticket.UpdateDetails(currentUser.UserId, command.Title, command.Description,
            command.Category, command.Priority, DateTime.UtcNow);
        TrackNewHistory(ticket, historyCount);
        await dbContext.SaveChangesAsync(cancellationToken); return TicketMappings.ToResponse(ticket);
    }

    public async Task<TicketResponse> ChangeStatusAsync(ChangeTicketStatusCommand command, CancellationToken cancellationToken)
    {
        EnsureCanManage();
        var ticket = await FindAsync(command.Id, cancellationToken);
        var historyCount = ticket.History.Count;
        ticket.ChangeStatus(currentUser.UserId, command.Status, DateTime.UtcNow);
        TrackNewHistory(ticket, historyCount);
        await dbContext.SaveChangesAsync(cancellationToken); return TicketMappings.ToResponse(ticket);
    }

    public async Task CancelAsync(CancelTicketCommand command, CancellationToken cancellationToken)
    {
        var ticket = await FindAsync(command.Id, cancellationToken); EnsureCanEdit(ticket);
        var historyCount = ticket.History.Count;
        ticket.Cancel(currentUser.UserId, DateTime.UtcNow);
        TrackNewHistory(ticket, historyCount);
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
        if (!currentUser.IsInRole(Roles.Admin) && !currentUser.IsInRole(Roles.Manager))
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

    private bool CanManage => currentUser.IsInRole(Roles.Admin) || currentUser.IsInRole(Roles.ITSupportSpecialist);
    private bool CanMonitorAll => CanManage || currentUser.IsInRole(Roles.Manager);
    private void EnsureCanManage() { if (!CanManage) throw new ForbiddenAccessException("Only support agents and administrators can manage ticket status."); }
    private void EnsureCanRead(Ticket ticket) { if (!CanMonitorAll && ticket.CreatedByUserId != currentUser.UserId) throw new ForbiddenAccessException("You cannot access this ticket."); }
    private void EnsureCanEdit(Ticket ticket)
    {
        if (ticket.IsCancelled)
            throw new InvalidOperationException("A cancelled ticket cannot be modified.");
        if (!CanManage && (ticket.CreatedByUserId != currentUser.UserId || ticket.Status != TicketStatus.Open))
            throw new ForbiddenAccessException("You cannot modify this ticket.");
    }
    private async Task<Ticket> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Tickets.FirstOrDefaultAsync(ticket => ticket.Id == id, cancellationToken)
        ?? throw new KeyNotFoundException($"Ticket '{id}' does not exist.");
    private static string GenerateReference(DateTime now) => $"HD-{now:yyyyMMdd}-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
    private void TrackNewHistory(Ticket ticket, int previousCount)
    {
        dbContext.TicketHistories.AddRange(ticket.History.Skip(previousCount));
    }
}
