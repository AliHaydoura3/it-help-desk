import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  downloadTicketAttachment,
  getAttachmentPolicy,
  getTicketAttachments,
  uploadTicketAttachment,
} from "../api/attachments";
import type { TicketAttachment } from "../types/attachment";
import { attachmentKeys } from "./attachmentKeys";

export function useAttachmentPolicy(enabled = true) {
  return useQuery({
    queryKey: attachmentKeys.policy,
    queryFn: getAttachmentPolicy,
    staleTime: 60 * 60 * 1000,
    enabled,
  });
}

export function useTicketAttachments(
  ticketId: string,
  pageNumber: number,
  pageSize: number,
) {
  return useQuery({
    queryKey: attachmentKeys.page(ticketId, pageNumber, pageSize),
    queryFn: () => getTicketAttachments(ticketId, pageNumber, pageSize),
    placeholderData: (previous) => previous,
  });
}

export function useUploadTicketAttachment(ticketId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: uploadTicketAttachment,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: attachmentKeys.ticket(ticketId) }),
        queryClient.invalidateQueries({ queryKey: ["tickets"] }),
      ]);
    },
  });
}

export function useDownloadTicketAttachment(ticketId: string) {
  return useMutation({
    mutationFn: (attachment: TicketAttachment) =>
      downloadTicketAttachment(ticketId, attachment),
  });
}
