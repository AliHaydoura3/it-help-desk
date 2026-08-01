import { ArrowUpRight, LoaderCircle } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { getApiErrorMessage } from "@/features/users/utils/getApiErrorMessage";
import { useWorkflowMutations } from "../../hooks/useTicketWorkflow";
import { ESCALATION_LEVELS, type Ticket, type TicketEscalationLevel } from "../../types/ticket";

const rank: Record<TicketEscalationLevel, number> = { None: 0, Level1: 1, Level2: 2, Level3: 3 };

export function EscalationPanel({ ticket }: { ticket: Ticket }) {
  const available = ESCALATION_LEVELS.filter((level) => rank[level] > rank[ticket.escalationLevel]);
  const [level, setLevel] = useState<TicketEscalationLevel>(available[0] ?? "Level3");
  const [reason, setReason] = useState("");
  const mutation = useWorkflowMutations(ticket.id).escalate;

  async function escalate() {
    try { await mutation.mutateAsync({ level, reason }); toast.success(`Ticket escalated to ${level}.`); setReason(""); }
    catch (error) { toast.error(getApiErrorMessage(error, "Unable to escalate this ticket.")); }
  }

  if (available.length === 0) return <div className="rounded-xl border bg-muted/30 p-5 text-sm text-muted-foreground">This ticket is already at the highest escalation level.</div>;
  return <div className="space-y-4"><div className="rounded-xl border px-4 py-3"><p className="text-xs text-muted-foreground">Current escalation</p><p className="mt-1 font-medium">{ticket.escalationLevel === "None" ? "Not escalated" : ticket.escalationLevel}</p></div><div className="space-y-2"><Label>Escalate to</Label><select className="h-9 w-full rounded-lg border bg-background px-3 text-sm" value={level} onChange={(event) => setLevel(event.target.value as TicketEscalationLevel)}>{available.map((item) => <option key={item}>{item}</option>)}</select></div><div className="space-y-2"><Label>Reason</Label><textarea className="min-h-28 w-full rounded-lg border bg-transparent px-3 py-2 text-sm outline-none focus:border-ring focus:ring-3 focus:ring-ring/30" maxLength={1000} placeholder="Explain why this ticket requires escalation..." value={reason} onChange={(event) => setReason(event.target.value)} /></div><Button disabled={!reason.trim() || mutation.isPending} onClick={escalate}>{mutation.isPending ? <LoaderCircle className="animate-spin" /> : <ArrowUpRight />} Escalate ticket</Button></div>;
}
