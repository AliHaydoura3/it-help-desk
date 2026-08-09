export interface SupportedAttachmentType {
  extension: string;
  contentType: string;
}

export interface AttachmentPolicy {
  maximumFileSizeBytes: number;
  maximumFilesPerTicket: number;
  supportedTypes: SupportedAttachmentType[];
}

export interface AttachmentUploader {
  id: string;
  firstName: string;
  lastName: string;
}

export interface TicketAttachment {
  id: string;
  ticketId: string;
  fileName: string;
  contentType: string;
  extension: string;
  sizeBytes: number;
  sha256Hash: string;
  uploadedBy: AttachmentUploader;
  uploadedAtUtc: string;
}

export interface TicketAttachmentsResponse {
  items: TicketAttachment[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface UploadAttachmentInput {
  ticketId: string;
  file: File;
  contentType: string;
  onProgress?: (percentage: number) => void;
}

export interface DownloadedAttachment {
  blob: Blob;
  fileName: string;
}
