import { ArrowUpRight, History, MessageSquareText, UserRoundCog, X } from "lucide-react";
import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import type { AuthUser } from "@/features/auth/types/auth";
import type { Ticket } from "../../types/ticket";
import { canManageWorkflow, canUseInternalNotes, canViewAssignmentHistory } from "../../utils/ticketPermissions";
import { AssignmentHistoryPanel } from "./AssignmentHistoryPanel";
import { AssignmentPanel } from "./AssignmentPanel";
import { EscalationPanel } from "./EscalationPanel";
import { InternalNotesPanel } from "./InternalNotesPanel";

type WorkflowTab = "assignment" | "escalation" | "notes" | "history";

export function TicketWorkflowDialog({ ticket, user, onClose }: { ticket: Ticket | null; user: AuthUser | null; onClose: () => void }) {
  const manage = canManageWorkflow(user);
  const notes = canUseInternalNotes(user);
  const history = canViewAssignmentHistory(user);
  const initialTab: WorkflowTab = manage ? "assignment" : "history";
  const [tab, setTab] = useState<WorkflowTab>(initialTab);
  useEffect(() => { if (ticket) setTab(manage ? "assignment" : "history"); }, [manage, ticket]);
  if (!ticket) return null;

  const tabs: Array<{ id: WorkflowTab; label: string; icon: typeof History }> = [];
  if (manage) tabs.push({ id: "assignment", label: "Assignment", icon: UserRoundCog });
  if (manage) tabs.push({ id: "escalation", label: "Escalation", icon: ArrowUpRight });
  if (notes) tabs.push({ id: "notes", label: "Internal notes", icon: MessageSquareText });
  if (history) tabs.push({ id: "history", label: "Assignment history", icon: History });

  return <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/35 p-4 backdrop-blur-sm"><section className="flex max-h-[88vh] w-full max-w-3xl flex-col overflow-hidden rounded-2xl bg-card shadow-2xl"><header className="flex items-start justify-between border-b px-6 py-5"><div><h2 className="text-xl font-semibold">Ticket workflow</h2><p className="mt-1 text-sm text-muted-foreground">{ticket.referenceNumber} · {ticket.title}</p></div><Button aria-label="Close workflow" onClick={onClose} size="icon" variant="ghost"><X /></Button></header><div className="flex gap-1 overflow-x-auto border-b px-4 py-2">{tabs.map(({ id, label, icon: Icon }) => <button className={cn("flex items-center gap-2 whitespace-nowrap rounded-lg px-3 py-2 text-sm transition-colors", tab === id ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:bg-muted hover:text-foreground")} key={id} onClick={() => setTab(id)}><Icon className="size-4" />{label}</button>)}</div><div className="overflow-y-auto p-6">{tab === "assignment" && manage && <AssignmentPanel ticket={ticket} />}{tab === "escalation" && manage && <EscalationPanel ticket={ticket} />}{tab === "notes" && notes && <InternalNotesPanel ticket={ticket} />}{tab === "history" && history && <AssignmentHistoryPanel ticket={ticket} />}</div></section></div>;
}
