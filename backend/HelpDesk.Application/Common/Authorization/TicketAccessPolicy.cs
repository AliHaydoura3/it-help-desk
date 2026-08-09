using HelpDesk.Application.Abstractions.Authentication;
using HelpDesk.Domain;

namespace HelpDesk.Application.Common.Authorization;

/// <summary>
/// Centralizes ticket access decisions so every adapter applies the same
/// ownership, assignment, and role rules.
/// </summary>
public static class TicketAccessPolicy
{
    public static bool CanReadTicket(this ICurrentUser currentUser, Ticket ticket) =>
        currentUser.HasPermission(Permission.MonitorAllTickets) ||
        ticket.CreatedByUserId == currentUser.UserId;

    public static bool CanEditTicket(this ICurrentUser currentUser, Ticket ticket) =>
        !ticket.IsCancelled &&
        (currentUser.HasPermission(Permission.EditAllTickets) ||
            (ticket.CreatedByUserId == currentUser.UserId &&
                ticket.Status == TicketStatus.Open));

    public static bool CanCancelTicket(this ICurrentUser currentUser, Ticket ticket) =>
        !ticket.IsCancelled &&
        (currentUser.HasPermission(Permission.CancelAllTickets) ||
            (ticket.CreatedByUserId == currentUser.UserId &&
                ticket.Status == TicketStatus.Open));

    public static bool CanChangeTicketStatus(this ICurrentUser currentUser, Ticket ticket) =>
        !ticket.IsCancelled &&
        (currentUser.HasPermission(Permission.ChangeAnyTicketStatus) ||
            (currentUser.HasPermission(Permission.ChangeAssignedTicketStatus) &&
                ticket.AssignedToUserId == currentUser.UserId));

    public static bool CanCommentOnTicket(this ICurrentUser currentUser, Ticket ticket) =>
        !ticket.IsCancelled &&
        ticket.Status != TicketStatus.Closed &&
        (currentUser.HasPermission(Permission.CommentOnAllTickets) ||
            ticket.CreatedByUserId == currentUser.UserId);

    public static bool CanUploadAttachment(this ICurrentUser currentUser, Ticket ticket) =>
        currentUser.CanCommentOnTicket(ticket);
}
