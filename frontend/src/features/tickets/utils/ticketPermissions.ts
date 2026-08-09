import type { AuthUser } from "@/features/auth/types/auth";
import { hasPermission } from "@/features/auth/authorization/roles";
import type { Ticket } from "../types/ticket";

export function canMonitorTickets(user: AuthUser | null): boolean {
  return hasPermission(user, "monitor-tickets");
}

export function canEditTicket(user: AuthUser | null, ticket: Ticket): boolean {
  return !ticket.isCancelled && (
    hasPermission(user, "edit-all-tickets") ||
    (ticket.createdByUserId === user?.id && ticket.status === "Open")
  );
}

export function canCancelTicket(user: AuthUser | null, ticket: Ticket): boolean {
  return !ticket.isCancelled && (
    hasPermission(user, "cancel-all-tickets") ||
    (ticket.createdByUserId === user?.id && ticket.status === "Open")
  );
}

export function canChangeTicketStatus(user: AuthUser | null, ticket: Ticket): boolean {
  return !ticket.isCancelled && (
    hasPermission(user, "change-any-ticket-status") ||
    (hasPermission(user, "change-assigned-ticket-status") && ticket.assignedToUserId === user?.id)
  );
}

export function canCommentOnTicket(user: AuthUser | null, ticket: Ticket): boolean {
  return !ticket.isCancelled && ticket.status !== "Closed" && (
    hasPermission(user, "comment-on-all-tickets") ||
    ticket.createdByUserId === user?.id
  );
}

export function canUploadAttachment(user: AuthUser | null, ticket: Ticket): boolean {
  return canCommentOnTicket(user, ticket);
}

export function canViewReports(user: AuthUser | null): boolean {
  return hasPermission(user, "view-ticket-reports");
}

export function canManageWorkflow(user: AuthUser | null): boolean {
  return hasPermission(user, "manage-ticket-workflow");
}

export function canViewAssignmentHistory(user: AuthUser | null): boolean {
  return hasPermission(user, "view-assignment-history");
}

export function canUseInternalNotes(user: AuthUser | null): boolean {
  return hasPermission(user, "use-internal-notes");
}
