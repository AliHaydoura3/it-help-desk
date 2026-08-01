import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  addInternalNote,
  assignTicket,
  autoAssignTicket,
  escalateTicket,
  getAssignableAgents,
  getAssignmentHistory,
  getInternalNotes,
} from "../api/ticketWorkflow";
import type { TicketEscalationLevel } from "../types/ticket";

const workflowKeys = {
  agents: ["ticket-workflow", "agents"] as const,
  assignments: (ticketId: string) => ["ticket-workflow", ticketId, "assignments"] as const,
  notes: (ticketId: string) => ["ticket-workflow", ticketId, "notes"] as const,
};

export function useAssignableAgents(enabled: boolean) {
  return useQuery({ queryKey: workflowKeys.agents, queryFn: getAssignableAgents, enabled, staleTime: 30_000 });
}

export function useAssignmentHistory(ticketId: string | null, enabled: boolean) {
  return useQuery({
    queryKey: workflowKeys.assignments(ticketId ?? ""),
    queryFn: () => getAssignmentHistory(ticketId!),
    enabled: ticketId !== null && enabled,
  });
}

export function useInternalNotes(ticketId: string | null, enabled: boolean) {
  return useQuery({
    queryKey: workflowKeys.notes(ticketId ?? ""),
    queryFn: () => getInternalNotes(ticketId!),
    enabled: ticketId !== null && enabled,
  });
}

export function useWorkflowMutations(ticketId: string) {
  const client = useQueryClient();
  const refreshTicket = async () => {
    await Promise.all([
      client.invalidateQueries({ queryKey: ["tickets"] }),
      client.invalidateQueries({ queryKey: workflowKeys.assignments(ticketId) }),
    ]);
  };

  return {
    assign: useMutation({ mutationFn: (agentId: string) => assignTicket(ticketId, agentId), onSuccess: refreshTicket }),
    autoAssign: useMutation({ mutationFn: () => autoAssignTicket(ticketId), onSuccess: refreshTicket }),
    escalate: useMutation({
      mutationFn: ({ level, reason }: { level: TicketEscalationLevel; reason: string }) =>
        escalateTicket(ticketId, level, reason),
      onSuccess: refreshTicket,
    }),
    addNote: useMutation({
      mutationFn: (content: string) => addInternalNote(ticketId, content),
      onSuccess: () => client.invalidateQueries({ queryKey: workflowKeys.notes(ticketId) }),
    }),
  };
}
