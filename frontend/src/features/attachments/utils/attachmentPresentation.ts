import {
  Archive,
  File,
  FileImage,
  FileSpreadsheet,
  FileText,
  type LucideIcon,
} from "lucide-react";
import type {
  AttachmentPolicy,
  DownloadedAttachment,
  SupportedAttachmentType,
} from "../types/attachment";

export function getAttachmentType(
  fileName: string,
  policy: AttachmentPolicy,
): SupportedAttachmentType | undefined {
  const extension = getExtension(fileName);
  return policy.supportedTypes.find(
    (type) => type.extension.toLowerCase() === extension,
  );
}

export function getAttachmentAccept(policy: AttachmentPolicy | undefined): string {
  return policy?.supportedTypes.map((type) => type.extension).join(",") ?? "";
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(bytes < 10 * 1024 ? 1 : 0)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

export function getFileIcon(extension: string): LucideIcon {
  const normalized = extension.toLowerCase();
  if ([".png", ".jpg", ".jpeg"].includes(normalized)) return FileImage;
  if ([".txt", ".log", ".csv", ".pdf", ".docx"].includes(normalized)) return FileText;
  if (normalized === ".xlsx") return FileSpreadsheet;
  if (normalized === ".zip") return Archive;
  return File;
}

export function saveAttachment(download: DownloadedAttachment): void {
  const url = URL.createObjectURL(download.blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = download.fileName;
  document.body.append(anchor);
  anchor.click();
  anchor.remove();
  URL.revokeObjectURL(url);
}

function getExtension(fileName: string): string {
  const index = fileName.lastIndexOf(".");
  return index < 0 ? "" : fileName.slice(index).toLowerCase();
}
