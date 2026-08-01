import { apiClient } from "@/shared/api/apiClient";
import type { Ticket, TicketFilters, TicketHistoryItem, TicketInput, TicketListResponse, TicketStatus, TicketSummary } from "../types/ticket";

export async function getTickets(filters: TicketFilters): Promise<TicketListResponse> {
  return (await apiClient.get<TicketListResponse>("/tickets", { params: filters })).data;
}

export async function createTicket(input: TicketInput): Promise<Ticket> {
  return (await apiClient.post<Ticket>("/tickets", input)).data;
}

export async function updateTicket(id: string, input: TicketInput): Promise<Ticket> {
  return (await apiClient.put<Ticket>(`/tickets/${id}`, input)).data;
}

export async function changeTicketStatus(id: string, status: TicketStatus): Promise<Ticket> {
  return (await apiClient.patch<Ticket>(`/tickets/${id}/status`, { status })).data;
}

export async function cancelTicket(id: string): Promise<void> {
  await apiClient.delete(`/tickets/${id}`);
}

export async function getTicketHistory(id: string): Promise<TicketHistoryItem[]> {
  return (await apiClient.get<TicketHistoryItem[]>(`/tickets/${id}/history`)).data;
}

export async function getTicketSummary(): Promise<TicketSummary> {
  return (await apiClient.get<TicketSummary>("/tickets/reports/summary")).data;
}
