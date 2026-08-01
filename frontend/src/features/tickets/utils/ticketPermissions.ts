import type { AuthUser } from "@/features/auth/types/auth";
import type { Ticket } from "../types/ticket";

const supportRoles = ["Admin", "ITSupportSpecialist"];

export function canManageTickets(user: AuthUser | null): boolean {
  return user?.roles.some((role) => supportRoles.includes(role)) ?? false;
}

export function canMonitorTickets(user: AuthUser | null): boolean {
  return canManageTickets(user) || (user?.roles.includes("Manager") ?? false);
}

export function canEditTicket(user: AuthUser | null, ticket: Ticket): boolean {
  return !ticket.isCancelled && (canManageTickets(user) || (ticket.createdByUserId === user?.id && ticket.status === "Open"));
}

export function canViewReports(user: AuthUser | null): boolean {
  return user?.roles.some((role) => role === "Admin" || role === "Manager") ?? false;
}

export function canManageWorkflow(user: AuthUser | null): boolean {
  return canManageTickets(user);
}

export function canViewAssignmentHistory(user: AuthUser | null): boolean {
  return canManageWorkflow(user) || (user?.roles.includes("Manager") ?? false);
}

export function canUseInternalNotes(user: AuthUser | null): boolean {
  return canManageWorkflow(user);
}
