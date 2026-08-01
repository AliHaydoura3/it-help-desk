import { LoaderCircle, TriangleAlert, X } from "lucide-react";

import { Button } from "@/components/ui/button";
import type { User } from "../types/user";

interface DeactivateUserDialogProps {
  user: User | null;
  isPending: boolean;
  onClose: () => void;
  onConfirm: () => Promise<void>;
}

export function DeactivateUserDialog({
  user,
  isPending,
  onClose,
  onConfirm,
}: DeactivateUserDialogProps) {
  if (!user) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/35 p-4 backdrop-blur-[2px]">
      <section
        aria-labelledby="deactivate-title"
        aria-modal="true"
        role="alertdialog"
        className="w-full max-w-md rounded-2xl bg-card p-6 shadow-2xl ring-1 ring-foreground/10"
      >
        <div className="flex items-start justify-between">
          <div className="flex size-11 items-center justify-center rounded-full bg-destructive/10 text-destructive">
            <TriangleAlert className="size-5" />
          </div>
          <Button aria-label="Close dialog" disabled={isPending} onClick={onClose} size="icon" variant="ghost">
            <X />
          </Button>
        </div>
        <h2 id="deactivate-title" className="mt-4 text-lg font-semibold">
          Deactivate this user?
        </h2>
        <p className="mt-2 text-sm leading-6 text-muted-foreground">
          <span className="font-medium text-foreground">
            {user.firstName} {user.lastName}
          </span>{" "}
          will no longer be able to sign in. Their account and history will be
          kept, and you can reactivate them later.
        </p>
        <div className="mt-6 flex justify-end gap-2">
          <Button disabled={isPending} onClick={onClose} variant="outline">
            Cancel
          </Button>
          <Button disabled={isPending} onClick={onConfirm} variant="destructive">
            {isPending && <LoaderCircle className="animate-spin" />}
            Deactivate
          </Button>
        </div>
      </section>
    </div>
  );
}
