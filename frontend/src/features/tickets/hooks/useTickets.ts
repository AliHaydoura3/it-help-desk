import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { cancelTicket, changeTicketStatus, createTicket, getTicketHistory, getTickets, getTicketSummary, updateTicket } from "../api/tickets";
import type { TicketFilters, TicketInput, TicketStatus } from "../types/ticket";

const allTicketsKey = ["tickets"] as const;
const ticketKeys = {
  all: allTicketsKey,
  list: (filters: TicketFilters) => [...allTicketsKey, "list", filters] as const,
  history: (id: string) => [...allTicketsKey, id, "history"] as const,
  summary: [...allTicketsKey, "summary"] as const,
};

export function useTickets(filters: TicketFilters) {
  return useQuery({ queryKey: ticketKeys.list(filters), queryFn: () => getTickets(filters), placeholderData: (previous) => previous });
}

export function useTicketSummary(enabled: boolean) {
  return useQuery({ queryKey: ticketKeys.summary, queryFn: getTicketSummary, enabled });
}

export function useTicketHistory(id: string | null) {
  return useQuery({ queryKey: ticketKeys.history(id ?? ""), queryFn: () => getTicketHistory(id!), enabled: id !== null });
}

export function useTicketMutations() {
  const client = useQueryClient();
  const refresh = () => client.invalidateQueries({ queryKey: ticketKeys.all });
  return {
    create: useMutation({ mutationFn: createTicket, onSuccess: refresh }),
    update: useMutation({ mutationFn: ({ id, input }: { id: string; input: TicketInput }) => updateTicket(id, input), onSuccess: refresh }),
    changeStatus: useMutation({ mutationFn: ({ id, status }: { id: string; status: TicketStatus }) => changeTicketStatus(id, status), onSuccess: refresh }),
    cancel: useMutation({ mutationFn: cancelTicket, onSuccess: refresh }),
  };
}
