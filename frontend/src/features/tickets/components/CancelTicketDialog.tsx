import { LoaderCircle, TriangleAlert, X } from "lucide-react";

import { Button } from "@/components/ui/button";
import type { Ticket } from "../types/ticket";

interface CancelTicketDialogProps {
  ticket: Ticket | null;
  isPending: boolean;
  onClose: () => void;
  onConfirm: () => Promise<void>;
}

export function CancelTicketDialog({
  ticket,
  isPending,
  onClose,
  onConfirm,
}: CancelTicketDialogProps) {
  if (!ticket) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/35 p-4 backdrop-blur-[2px]">
      <section
        aria-describedby="cancel-ticket-description"
        aria-labelledby="cancel-ticket-title"
        aria-modal="true"
        className="w-full max-w-md rounded-2xl bg-card p-6 shadow-2xl ring-1 ring-foreground/10"
        role="alertdialog"
      >
        <div className="flex items-start justify-between">
          <div className="flex size-11 items-center justify-center rounded-full bg-destructive/10 text-destructive">
            <TriangleAlert className="size-5" />
          </div>
          <Button
            aria-label="Close dialog"
            disabled={isPending}
            onClick={onClose}
            size="icon"
            variant="ghost"
          >
            <X />
          </Button>
        </div>

        <h2 className="mt-4 text-lg font-semibold" id="cancel-ticket-title">
          Cancel this ticket?
        </h2>
        <p className="mt-2 text-sm leading-6 text-muted-foreground" id="cancel-ticket-description">
          <span className="font-medium text-foreground">{ticket.referenceNumber}</span>
          {" — "}
          {ticket.title} will be removed from the active ticket list. Its history will be preserved for auditing.
        </p>

        <div className="mt-6 flex justify-end gap-2">
          <Button disabled={isPending} onClick={onClose} variant="outline">
            Keep ticket
          </Button>
          <Button disabled={isPending} onClick={onConfirm} variant="destructive">
            {isPending && <LoaderCircle className="animate-spin" />}
            Cancel ticket
          </Button>
        </div>
      </section>
    </div>
  );
}
