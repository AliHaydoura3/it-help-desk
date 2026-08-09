import {
  CheckCircle2,
  Download,
  FileUp,
  LoaderCircle,
  Paperclip,
  ShieldCheck,
  Upload,
  X,
} from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import type { AuthUser } from "@/features/auth/types/auth";
import type { Ticket } from "@/features/tickets/types/ticket";
import { canUploadAttachment } from "@/features/tickets/utils/ticketPermissions";
import { getApiErrorMessage } from "@/features/users/utils/getApiErrorMessage";
import { cn } from "@/lib/utils";
import {
  useAttachmentPolicy,
  useDownloadTicketAttachment,
  useTicketAttachments,
  useUploadTicketAttachment,
} from "../hooks/useAttachments";
import type { TicketAttachment } from "../types/attachment";
import {
  formatFileSize,
  getAttachmentAccept,
  getAttachmentType,
  getFileIcon,
  saveAttachment,
} from "../utils/attachmentPresentation";
import { AttachmentPagination } from "./AttachmentPagination";

const PAGE_SIZE = 10;

export function TicketAttachmentsPanel({
  ticket,
  user,
}: {
  ticket: Ticket;
  user: AuthUser | null;
}) {
  const [page, setPage] = useState(1);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [dragActive, setDragActive] = useState(false);
  const [downloadingId, setDownloadingId] = useState<string | null>(null);
  const uploadable = canUploadAttachment(user, ticket);
  const policyQuery = useAttachmentPolicy(uploadable);
  const attachmentsQuery = useTicketAttachments(ticket.id, page, PAGE_SIZE);
  const uploadMutation = useUploadTicketAttachment(ticket.id);
  const downloadMutation = useDownloadTicketAttachment(ticket.id);
  const policy = policyQuery.data;

  useEffect(() => {
    const totalPages = attachmentsQuery.data?.totalPages;
    if (totalPages && page > totalPages) setPage(totalPages);
  }, [attachmentsQuery.data?.totalPages, page]);

  function selectFile(file: File | undefined) {
    if (!file) return;
    if (!policy) {
      toast.error("Attachment rules are still loading. Please try again.");
      return;
    }
    if (file.size === 0) {
      toast.error("The selected file is empty.");
      return;
    }
    if (file.size > policy.maximumFileSizeBytes) {
      toast.error(`Files must not exceed ${formatFileSize(policy.maximumFileSizeBytes)}.`);
      return;
    }
    if (!getAttachmentType(file.name, policy)) {
      toast.error("This file type is not supported.");
      return;
    }
    if ((attachmentsQuery.data?.totalCount ?? 0) >= policy.maximumFilesPerTicket) {
      toast.error(`This ticket already has the maximum of ${policy.maximumFilesPerTicket} attachments.`);
      return;
    }
    setSelectedFile(file);
    setUploadProgress(0);
  }

  async function upload() {
    if (!selectedFile || !policy) return;
    const supportedType = getAttachmentType(selectedFile.name, policy);
    if (!supportedType) return;

    try {
      await uploadMutation.mutateAsync({
        ticketId: ticket.id,
        file: selectedFile,
        contentType: supportedType.contentType,
        onProgress: setUploadProgress,
      });
      setSelectedFile(null);
      setUploadProgress(0);
      setPage(1);
      toast.success("Attachment uploaded securely.");
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Unable to upload this attachment."));
    }
  }

  async function download(attachment: TicketAttachment) {
    setDownloadingId(attachment.id);
    try {
      saveAttachment(await downloadMutation.mutateAsync(attachment));
      toast.success("Download started.");
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Unable to download this attachment."));
    } finally {
      setDownloadingId(null);
    }
  }

  return (
    <div className="flex min-h-0 flex-1 flex-col bg-muted/20">
      {uploadable && (
        <div className="border-b bg-card p-4 sm:px-6">
          {policyQuery.isError ? (
            <div className="rounded-xl border border-destructive/20 bg-destructive/5 p-4 text-sm text-destructive">
              Attachment rules could not be loaded. Uploads are temporarily unavailable.
            </div>
          ) : (
            <div
              className={cn(
                "rounded-xl border border-dashed bg-muted/20 p-4 transition-colors",
                dragActive && "border-primary bg-primary/5",
              )}
              onDragEnter={(event) => { event.preventDefault(); setDragActive(true); }}
              onDragLeave={(event) => { event.preventDefault(); setDragActive(false); }}
              onDragOver={(event) => event.preventDefault()}
              onDrop={(event) => {
                event.preventDefault();
                setDragActive(false);
                selectFile(event.dataTransfer.files[0]);
              }}
            >
              {selectedFile ? (
                <div>
                  <div className="flex items-center gap-3">
                    <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-primary/8 text-primary"><FileUp className="size-5" /></div>
                    <div className="min-w-0 flex-1"><p className="truncate text-sm font-medium">{selectedFile.name}</p><p className="text-xs text-muted-foreground">{formatFileSize(selectedFile.size)}</p></div>
                    <Button aria-label="Remove selected file" disabled={uploadMutation.isPending} onClick={() => setSelectedFile(null)} size="icon-sm" variant="ghost"><X /></Button>
                    <Button disabled={uploadMutation.isPending} onClick={upload}>
                      {uploadMutation.isPending ? <LoaderCircle className="animate-spin" /> : <Upload />}
                      Upload
                    </Button>
                  </div>
                  {uploadMutation.isPending && (
                    <div className="mt-3">
                      <div className="mb-1 flex justify-between text-[11px] text-muted-foreground"><span>Uploading and validating…</span><span>{uploadProgress}%</span></div>
                      <div className="h-1.5 overflow-hidden rounded-full bg-muted"><div className="h-full rounded-full bg-primary transition-[width]" style={{ width: `${uploadProgress}%` }} /></div>
                    </div>
                  )}
                </div>
              ) : (
                <label className={cn("flex cursor-pointer flex-col items-center py-3 text-center", (!policy || policyQuery.isLoading) && "pointer-events-none opacity-60")}>
                  {policyQuery.isLoading ? <LoaderCircle className="size-6 animate-spin text-muted-foreground" /> : <Paperclip className="size-6 text-muted-foreground" />}
                  <span className="mt-2 text-sm font-medium">Drop a file here or browse</span>
                  <span className="mt-1 text-xs text-muted-foreground">
                    {policy ? `${policy.supportedTypes.map((type) => type.extension).join(", ")} · Up to ${formatFileSize(policy.maximumFileSizeBytes)}` : "Loading attachment rules…"}
                  </span>
                  <input accept={getAttachmentAccept(policy)} className="sr-only" disabled={!policy} onChange={(event) => { selectFile(event.target.files?.[0]); event.target.value = ""; }} type="file" />
                </label>
              )}
            </div>
          )}
        </div>
      )}

      <div className="min-h-56 flex-1 overflow-y-auto p-4 sm:p-6">
        {attachmentsQuery.isLoading ? (
          <div className="flex min-h-56 items-center justify-center"><LoaderCircle className="animate-spin text-muted-foreground" /></div>
        ) : attachmentsQuery.isError ? (
          <div className="flex min-h-56 flex-col items-center justify-center text-center"><p className="font-medium">Could not load attachments</p><p className="mt-1 text-sm text-muted-foreground">Check your connection and try again.</p><Button className="mt-4" onClick={() => attachmentsQuery.refetch()} variant="outline">Try again</Button></div>
        ) : attachmentsQuery.data?.items.length === 0 ? (
          <div className="flex min-h-56 flex-col items-center justify-center text-center"><div className="flex size-12 items-center justify-center rounded-full bg-primary/8 text-primary"><Paperclip className="size-5" /></div><p className="mt-4 font-medium">No attachments yet</p><p className="mt-1 max-w-sm text-sm text-muted-foreground">Screenshots, documents, and diagnostic logs attached to this ticket will appear here.</p></div>
        ) : (
          <div className="space-y-3">
            {attachmentsQuery.data?.items.map((attachment) => <AttachmentItem attachment={attachment} downloading={downloadingId === attachment.id} key={attachment.id} onDownload={download} />)}
          </div>
        )}
      </div>

      {attachmentsQuery.data && (
        <AttachmentPagination disabled={attachmentsQuery.isFetching} onPageChange={setPage} pageNumber={attachmentsQuery.data.pageNumber} pageSize={attachmentsQuery.data.pageSize} totalCount={attachmentsQuery.data.totalCount} totalPages={attachmentsQuery.data.totalPages} />
      )}

      {!uploadable && (
        <div className="flex items-center justify-center gap-2 border-t bg-card px-6 py-3 text-xs text-muted-foreground">
          <ShieldCheck className="size-4" /> Attachments are available for secure download. Upload access is read-only.
        </div>
      )}
    </div>
  );
}

function AttachmentItem({
  attachment,
  downloading,
  onDownload,
}: {
  attachment: TicketAttachment;
  downloading: boolean;
  onDownload: (attachment: TicketAttachment) => void;
}) {
  const Icon = getFileIcon(attachment.extension);
  return (
    <article className="flex items-center gap-3 rounded-xl border bg-card p-3 shadow-xs sm:p-4">
      <div className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-muted text-muted-foreground"><Icon className="size-5" /></div>
      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium" title={attachment.fileName}>{attachment.fileName}</p>
        <p className="mt-1 truncate text-xs text-muted-foreground">
          {formatFileSize(attachment.sizeBytes)} · {attachment.uploadedBy.firstName} {attachment.uploadedBy.lastName} · {new Date(attachment.uploadedAtUtc).toLocaleString()}
        </p>
        <p className="mt-1 flex items-center gap-1 text-[10px] text-muted-foreground" title={`SHA-256: ${attachment.sha256Hash}`}><CheckCircle2 className="size-3 text-emerald-600" /> Integrity verified · {attachment.sha256Hash.slice(0, 12)}…</p>
      </div>
      <Button aria-label={`Download ${attachment.fileName}`} disabled={downloading} onClick={() => onDownload(attachment)} size="icon" variant="outline">
        {downloading ? <LoaderCircle className="animate-spin" /> : <Download />}
      </Button>
    </article>
  );
}
