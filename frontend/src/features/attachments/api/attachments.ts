import { apiClient } from "@/shared/api/apiClient";
import type {
  AttachmentPolicy,
  DownloadedAttachment,
  TicketAttachment,
  TicketAttachmentsResponse,
  UploadAttachmentInput,
} from "../types/attachment";

export async function getAttachmentPolicy(): Promise<AttachmentPolicy> {
  return (await apiClient.get<AttachmentPolicy>("/attachments/policy")).data;
}

export async function getTicketAttachments(
  ticketId: string,
  pageNumber: number,
  pageSize: number,
): Promise<TicketAttachmentsResponse> {
  return (await apiClient.get<TicketAttachmentsResponse>(
    `/tickets/${ticketId}/attachments`,
    { params: { pageNumber, pageSize } },
  )).data;
}

export async function uploadTicketAttachment(
  input: UploadAttachmentInput,
): Promise<TicketAttachment> {
  const form = new FormData();
  const typedContent = input.file.slice(0, input.file.size, input.contentType);
  form.append("file", typedContent, input.file.name);

  return (await apiClient.post<TicketAttachment>(
    `/tickets/${input.ticketId}/attachments`,
    form,
    {
      headers: { "Content-Type": undefined },
      onUploadProgress: (event) => {
        const total = event.total ?? input.file.size;
        if (total > 0) input.onProgress?.(Math.min(100, Math.round(event.loaded / total * 100)));
      },
    },
  )).data;
}

export async function downloadTicketAttachment(
  ticketId: string,
  attachment: TicketAttachment,
): Promise<DownloadedAttachment> {
  const response = await apiClient.get<Blob>(
    `/tickets/${ticketId}/attachments/${attachment.id}/download`,
    { responseType: "blob" },
  );
  return { blob: response.data, fileName: attachment.fileName };
}
