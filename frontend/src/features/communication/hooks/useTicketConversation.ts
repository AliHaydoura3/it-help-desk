import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { addTicketComment, getMentionableAgents, getTicketComments } from "../api/communication";
import type { AddTicketCommentInput } from "../types/communication";
import { communicationKeys } from "./communicationKeys";

export function useTicketComments(
  ticketId: string | null,
  pageNumber: number,
  pageSize: number,
) {
  return useQuery({
    queryKey: communicationKeys.commentsPage(ticketId ?? "", pageNumber, pageSize),
    queryFn: () => getTicketComments(ticketId!, pageNumber, pageSize),
    enabled: ticketId !== null,
  });
}

export function useMentionableAgents(ticketId: string | null, search: string, enabled: boolean) {
  return useQuery({
    queryKey: communicationKeys.mentions(ticketId ?? "", search),
    queryFn: () => getMentionableAgents(ticketId!, search.trim() || undefined),
    enabled: ticketId !== null && enabled,
  });
}

export function useAddTicketComment(ticketId: string | null) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: AddTicketCommentInput) => addTicketComment(ticketId!, input),
    onSuccess: async () => {
      if (!ticketId) return;
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: communicationKeys.comments(ticketId) }),
        queryClient.invalidateQueries({ queryKey: communicationKeys.notifications }),
      ]);
    },
  });
}
