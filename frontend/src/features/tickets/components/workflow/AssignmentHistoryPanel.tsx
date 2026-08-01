import { ArrowRight, LoaderCircle, RotateCcw, UserRoundCheck } from "lucide-react";
import { useAssignmentHistory } from "../../hooks/useTicketWorkflow";
import type { Ticket } from "../../types/ticket";

export function AssignmentHistoryPanel({ ticket }: { ticket: Ticket }) {
  const history = useAssignmentHistory(ticket.id, true);
  if (history.isLoading) return <LoaderCircle className="mx-auto animate-spin" />;
  if (history.isError) return <p className="py-8 text-center text-sm text-destructive">Unable to load assignment history.</p>;
  if (!history.data?.length) return <p className="py-8 text-center text-sm text-muted-foreground">This ticket has not been assigned yet.</p>;
  return <div className="space-y-3">{history.data.map((item, index) => <article className="flex gap-3 rounded-xl border p-4" key={`${item.occurredAtUtc}-${index}`}><div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-muted">{item.assignmentType === "Reassignment" ? <RotateCcw className="size-4" /> : <UserRoundCheck className="size-4" />}</div><div className="min-w-0"><p className="text-sm font-medium">{item.assignmentType}</p><div className="mt-1 flex items-center gap-1 text-xs text-muted-foreground">{item.previousAgentId && <><span>{item.previousAgentId.slice(0, 8)}</span><ArrowRight className="size-3" /></>}<span>{item.assignedAgentId.slice(0, 8)}</span></div><p className="mt-1 text-[11px] text-muted-foreground">{new Date(item.occurredAtUtc).toLocaleString()}</p></div></article>)}</div>;
}
