import { apiClient } from "@/shared/api/apiClient";
import type {
  AssignableAgent,
  AssignmentHistoryItem,
  InternalNote,
  Ticket,
  TicketEscalationLevel,
} from "../types/ticket";

export async function getAssignableAgents(): Promise<AssignableAgent[]> {
  return (await apiClient.get<AssignableAgent[]>("/tickets/assignable-agents")).data;
}

export async function assignTicket(ticketId: string, agentUserId: string): Promise<Ticket> {
  return (await apiClient.post<Ticket>(`/tickets/${ticketId}/assign`, { agentUserId })).data;
}

export async function autoAssignTicket(ticketId: string): Promise<Ticket> {
  return (await apiClient.post<Ticket>(`/tickets/${ticketId}/auto-assign`)).data;
}

export async function escalateTicket(
  ticketId: string,
  level: TicketEscalationLevel,
  reason: string,
): Promise<Ticket> {
  return (await apiClient.post<Ticket>(`/tickets/${ticketId}/escalate`, { level, reason })).data;
}

export async function getAssignmentHistory(ticketId: string): Promise<AssignmentHistoryItem[]> {
  return (await apiClient.get<AssignmentHistoryItem[]>(`/tickets/${ticketId}/assignments`)).data;
}

export async function getInternalNotes(ticketId: string): Promise<InternalNote[]> {
  return (await apiClient.get<InternalNote[]>(`/tickets/${ticketId}/internal-notes`)).data;
}

export async function addInternalNote(ticketId: string, content: string): Promise<InternalNote> {
  return (await apiClient.post<InternalNote>(`/tickets/${ticketId}/internal-notes`, { content })).data;
}
