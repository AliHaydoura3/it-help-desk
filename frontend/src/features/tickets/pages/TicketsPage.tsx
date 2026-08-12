import { useEffect, useState } from "react";
import { Plus, Search } from "lucide-react";
import { useSearchParams } from "react-router-dom";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { TicketConversationDialog } from "@/features/communication/components/TicketConversationDialog";
import { getApiErrorMessage } from "@/features/users/utils/getApiErrorMessage";
import { CancelTicketDialog } from "../components/CancelTicketDialog";
import { TicketFormDialog } from "../components/TicketFormDialog";
import { TicketHistoryDialog } from "../components/TicketHistoryDialog";
import { TicketPagination } from "../components/TicketPagination";
import { TicketTable } from "../components/TicketTable";
import { TicketWorkflowDialog } from "../components/workflow/TicketWorkflowDialog";
import { useTicket, useTicketMutations, useTickets, useTicketSummary } from "../hooks/useTickets";
import { TICKET_CATEGORIES, TICKET_PRIORITIES, TICKET_STATUSES, type Ticket, type TicketCategory, type TicketPriority, type TicketStatus } from "../types/ticket";
import { canViewReports } from "../utils/ticketPermissions";
import type { TicketFormData } from "../validation/ticketSchema";

const PAGE_SIZE = 10;

export default function TicketsPage() {
  const auth = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [category, setCategory] = useState<TicketCategory | "">("");
  const [priority, setPriority] = useState<TicketPriority | "">("");
  const [status, setStatus] = useState<TicketStatus | "">("");
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<Ticket | null>(null);
  const [ticketToCancel, setTicketToCancel] = useState<Ticket | null>(null);
  const [historyTicket, setHistoryTicket] = useState<Ticket | null>(null);
  const [workflowTicket, setWorkflowTicket] = useState<Ticket | null>(null);
  const [conversationTicket, setConversationTicket] = useState<Ticket | null>(null);
  const linkedTicketId = searchParams.get("conversation");
  const linkedTicket = useTicket(linkedTicketId);

  useEffect(() => { const id = window.setTimeout(() => setDebouncedSearch(search.trim()), 300); return () => window.clearTimeout(id); }, [search]);
  const query = useTickets({ pageNumber: page, pageSize: PAGE_SIZE, search: debouncedSearch || undefined, category: category || undefined, priority: priority || undefined, status: status || undefined });
  const summary = useTicketSummary(canViewReports(auth.user));
  const mutations = useTicketMutations();
  const pending = mutations.create.isPending || mutations.update.isPending || mutations.changeStatus.isPending || mutations.cancel.isPending;

  useEffect(() => {
    if (query.data?.totalPages === undefined) return;
    const lastPage = Math.max(1, query.data.totalPages);
    if (page > lastPage) setPage(lastPage);
  }, [page, query.data?.totalPages]);

  useEffect(() => {
    if (linkedTicket.data) setConversationTicket(linkedTicket.data);
  }, [linkedTicket.data]);

  useEffect(() => {
    if (!linkedTicketId || !linkedTicket.isError) return;
    toast.error("This ticket could not be opened.");
    const next = new URLSearchParams(searchParams);
    next.delete("conversation");
    setSearchParams(next, { replace: true });
  }, [linkedTicket.isError, linkedTicketId, searchParams, setSearchParams]);

  function resetPage() { setPage(1); }
  async function submit(data: TicketFormData) {
    try {
      if (editing) await mutations.update.mutateAsync({ id: editing.id, input: data });
      else await mutations.create.mutateAsync(data);
      toast.success(editing ? "Ticket updated." : "Ticket created."); setFormOpen(false); setEditing(null);
    } catch (error) { toast.error(getApiErrorMessage(error, "Unable to save the ticket.")); }
  }
  async function confirmCancellation() {
    if (!ticketToCancel) return;
    try {
      await mutations.cancel.mutateAsync(ticketToCancel.id);
      toast.success(`${ticketToCancel.referenceNumber} cancelled.`);
      setTicketToCancel(null);
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Unable to cancel the ticket."));
    }
  }
  async function changeStatus(ticket: Ticket, nextStatus: TicketStatus) {
    try { await mutations.changeStatus.mutateAsync({ id: ticket.id, status: nextStatus }); toast.success("Status updated."); } catch (error) { toast.error(getApiErrorMessage(error)); }
  }
  return <div>
    <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6">
      <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end"><div><p className="text-sm font-medium text-muted-foreground">{getWorkspaceLabel(auth.user?.role)}</p><h1 className="mt-1 text-3xl font-semibold tracking-tight">Support tickets</h1><p className="mt-2 text-sm text-muted-foreground">Create, track, and manage help desk requests.</p></div><Button className="h-10 px-4" onClick={() => { setEditing(null); setFormOpen(true); }}><Plus /> New ticket</Button></div>

      {canViewReports(auth.user) && summary.data && <div className="mt-7 grid gap-3 sm:grid-cols-2 lg:grid-cols-4"><Stat label="Total" value={summary.data.total} /><Stat label="Open" value={summary.data.open} /><Stat label="In progress" value={summary.data.inProgress} /><Stat label="Critical" value={summary.data.critical} /></div>}

      <Card className="mt-7 gap-0 py-0 shadow-sm"><div className="grid gap-3 border-b p-4 md:grid-cols-[minmax(240px,1fr)_repeat(3,160px)]"><div className="relative"><Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" /><Input className="h-9 pl-9" placeholder="Search title or reference..." value={search} onChange={(event) => { setSearch(event.target.value); resetPage(); }} /></div><Filter value={category} onChange={(value) => { setCategory(value as TicketCategory | ""); resetPage(); }} label="All categories" options={TICKET_CATEGORIES} /><Filter value={priority} onChange={(value) => { setPriority(value as TicketPriority | ""); resetPage(); }} label="All priorities" options={TICKET_PRIORITIES} /><Filter value={status} onChange={(value) => { setStatus(value as TicketStatus | ""); resetPage(); }} label="All statuses" options={TICKET_STATUSES} /></div>
        {query.isLoading ? <div className="py-20 text-center text-sm text-muted-foreground">Loading tickets…</div> : query.isError ? <div className="py-20 text-center text-sm text-destructive">Unable to load tickets.</div> : <TicketTable tickets={query.data?.items ?? []} user={auth.user} pending={pending} onEdit={(ticket) => { setEditing(ticket); setFormOpen(true); }} onCancel={setTicketToCancel} onConversation={setConversationTicket} onHistory={setHistoryTicket} onWorkflow={setWorkflowTicket} onStatus={changeStatus} />}
        {query.data && (
          <TicketPagination
            disabled={query.isFetching}
            onPageChange={setPage}
            pageNumber={page}
            pageSize={query.data.pageSize}
            totalCount={query.data.totalCount}
            totalPages={query.data.totalPages}
          />
        )}
      </Card>
    </main>
    <TicketFormDialog open={formOpen} ticket={editing} pending={pending} onClose={() => { setFormOpen(false); setEditing(null); }} onSubmit={submit} />
    <CancelTicketDialog
      isPending={mutations.cancel.isPending}
      onClose={() => setTicketToCancel(null)}
      onConfirm={confirmCancellation}
      ticket={ticketToCancel}
    />
    <TicketHistoryDialog ticket={historyTicket} onClose={() => setHistoryTicket(null)} />
    <TicketConversationDialog
      ticket={conversationTicket}
      user={auth.user}
      onClose={() => {
        setConversationTicket(null);
        if (searchParams.has("conversation")) {
          const next = new URLSearchParams(searchParams);
          next.delete("conversation");
          setSearchParams(next, { replace: true });
        }
      }}
    />
    <TicketWorkflowDialog
      ticket={query.data?.items.find((ticket) => ticket.id === workflowTicket?.id) ?? workflowTicket}
      user={auth.user}
      onClose={() => setWorkflowTicket(null)}
    />
  </div>;
}

function Filter({ value, label, options, onChange }: { value: string; label: string; options: readonly string[]; onChange: (value: string) => void }) { return <select className="h-9 rounded-lg border bg-background px-3 text-sm" value={value} onChange={(event) => onChange(event.target.value)}><option value="">{label}</option>{options.map((item) => <option key={item} value={item}>{item.replace(/([A-Z])/g, " $1").trim()}</option>)}</select>; }
function Stat({ label, value }: { label: string; value: number }) { return <Card className="gap-0 py-0"><CardContent className="p-4"><p className="text-2xl font-semibold">{value}</p><p className="mt-1 text-xs text-muted-foreground">{label}</p></CardContent></Card>; }
function getWorkspaceLabel(role: string | undefined): string {
  if (role === "Manager") return "Read-only ticket oversight";
  if (role === "Admin" || role === "ITSupportSpecialist") return "Support queue";
  return "My requests";
}
