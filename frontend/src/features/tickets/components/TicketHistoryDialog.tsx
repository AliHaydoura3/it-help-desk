import { Clock3, LoaderCircle, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useTicketHistory } from "../hooks/useTickets";
import type { Ticket } from "../types/ticket";

export function TicketHistoryDialog({ ticket, onClose }: { ticket: Ticket | null; onClose: () => void }) {
  const query = useTicketHistory(ticket?.id ?? null);
  if (!ticket) return null;
  return <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/35 p-4 backdrop-blur-sm">
    <section className="max-h-[80vh] w-full max-w-lg overflow-y-auto rounded-2xl bg-card shadow-2xl">
      <header className="sticky top-0 flex items-start justify-between border-b bg-card p-5"><div><h2 className="text-lg font-semibold">Ticket history</h2><p className="mt-1 text-xs text-muted-foreground">{ticket.referenceNumber}</p></div><Button onClick={onClose} size="icon" variant="ghost"><X /></Button></header>
      <div className="p-5">{query.isLoading ? <LoaderCircle className="mx-auto animate-spin" /> : <div className="space-y-5">{query.data?.map((item, index) => <div className="relative flex gap-3" key={`${item.occurredAtUtc}-${index}`}><div className="flex size-8 shrink-0 items-center justify-center rounded-full bg-muted"><Clock3 className="size-4" /></div><div><p className="text-sm font-medium">{item.action}</p>{item.newValue && <p className="mt-1 text-xs text-muted-foreground">{item.previousValue ? `${item.previousValue} → ` : ""}{item.newValue}</p>}<p className="mt-1 text-[11px] text-muted-foreground">{new Date(item.occurredAtUtc).toLocaleString()}</p></div></div>)}</div>}</div>
    </section>
  </div>;
}
