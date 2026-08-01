import { zodResolver } from "@hookform/resolvers/zod";
import { LoaderCircle, X } from "lucide-react";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { TICKET_CATEGORIES, TICKET_PRIORITIES, type Ticket } from "../types/ticket";
import { ticketSchema, type TicketFormData } from "../validation/ticketSchema";

export function TicketFormDialog({ open, ticket, pending, onClose, onSubmit }: { open: boolean; ticket: Ticket | null; pending: boolean; onClose: () => void; onSubmit: (data: TicketFormData) => Promise<void> }) {
  const { register, handleSubmit, reset, formState: { errors } } = useForm<TicketFormData>({
    resolver: zodResolver(ticketSchema),
    defaultValues: { title: "", description: "", category: "Hardware", priority: "Medium" },
  });
  useEffect(() => {
    if (open) reset(ticket ? { title: ticket.title, description: ticket.description, category: ticket.category, priority: ticket.priority } : { title: "", description: "", category: "Hardware", priority: "Medium" });
  }, [open, reset, ticket]);
  if (!open) return null;

  return <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/35 p-4 backdrop-blur-sm">
    <section className="w-full max-w-xl rounded-2xl bg-card shadow-2xl" role="dialog" aria-modal="true">
      <header className="flex items-start justify-between border-b p-5"><div><h2 className="text-xl font-semibold">{ticket ? "Edit ticket" : "Create support ticket"}</h2><p className="mt-1 text-sm text-muted-foreground">Provide enough detail for the support team to investigate.</p></div><Button onClick={onClose} size="icon" variant="ghost"><X /></Button></header>
      <form onSubmit={handleSubmit(onSubmit)}>
        <div className="space-y-4 p-5">
          <Field label="Title" error={errors.title?.message}><Input autoFocus placeholder="Briefly describe the issue" {...register("title")} /></Field>
          <Field label="Description" error={errors.description?.message}><textarea className="min-h-32 w-full rounded-lg border border-input bg-transparent px-3 py-2 text-sm outline-none focus:border-ring focus:ring-3 focus:ring-ring/30" placeholder="What happened, when did it start, and what have you tried?" {...register("description")} /></Field>
          <div className="grid gap-4 sm:grid-cols-2">
            <Field label="Category"><select className="h-9 w-full rounded-lg border bg-background px-3 text-sm" {...register("category")}>{TICKET_CATEGORIES.map((value) => <option key={value} value={value}>{value === "AccessRequest" ? "Access Request" : value}</option>)}</select></Field>
            <Field label="Priority"><select className="h-9 w-full rounded-lg border bg-background px-3 text-sm" {...register("priority")}>{TICKET_PRIORITIES.map((value) => <option key={value}>{value}</option>)}</select></Field>
          </div>
        </div>
        <footer className="flex justify-end gap-2 border-t bg-muted/30 p-4"><Button disabled={pending} onClick={onClose} type="button" variant="outline">Cancel</Button><Button disabled={pending} type="submit">{pending && <LoaderCircle className="animate-spin" />}{ticket ? "Save changes" : "Create ticket"}</Button></footer>
      </form>
    </section>
  </div>;
}

function Field({ label, error, children }: { label: string; error?: string; children: React.ReactNode }) {
  return <div className="space-y-2"><Label>{label}</Label>{children}{error && <p className="text-sm text-destructive">{error}</p>}</div>;
}
