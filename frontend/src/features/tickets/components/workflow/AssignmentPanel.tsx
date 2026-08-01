import { Bot, LoaderCircle, UserRoundCheck } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { getApiErrorMessage } from "@/features/users/utils/getApiErrorMessage";
import { useAssignableAgents, useWorkflowMutations } from "../../hooks/useTicketWorkflow";
import type { Ticket } from "../../types/ticket";

export function AssignmentPanel({ ticket }: { ticket: Ticket }) {
  const agents = useAssignableAgents(true);
  const mutations = useWorkflowMutations(ticket.id);
  const [agentId, setAgentId] = useState(ticket.assignedToUserId ?? "");
  useEffect(() => setAgentId(ticket.assignedToUserId ?? ""), [ticket.assignedToUserId]);

  async function assign() {
    if (!agentId) return;
    try { await mutations.assign.mutateAsync(agentId); toast.success(ticket.assignedToUserId ? "Ticket reassigned." : "Ticket assigned."); }
    catch (error) { toast.error(getApiErrorMessage(error, "Unable to assign this ticket.")); }
  }
  async function autoAssign() {
    try { const updated = await mutations.autoAssign.mutateAsync(); setAgentId(updated.assignedToUserId ?? ""); toast.success("Ticket assigned to the least-loaded agent."); }
    catch (error) { toast.error(getApiErrorMessage(error, "Automatic assignment failed.")); }
  }

  return <div className="space-y-5">
    <div><h3 className="font-medium">Manual assignment</h3><p className="mt-1 text-xs text-muted-foreground">Select an active support agent. Assigning another agent records a reassignment.</p></div>
    <div className="flex flex-col gap-2 sm:flex-row"><select className="h-9 flex-1 rounded-lg border bg-background px-3 text-sm" disabled={agents.isLoading} value={agentId} onChange={(event) => setAgentId(event.target.value)}><option value="">Select a support agent</option>{agents.data?.map((agent) => <option value={agent.id} key={agent.id}>{agent.firstName} {agent.lastName} · {agent.activeTicketCount} active</option>)}</select><Button disabled={!agentId || mutations.assign.isPending || agentId === ticket.assignedToUserId} onClick={assign}>{mutations.assign.isPending ? <LoaderCircle className="animate-spin" /> : <UserRoundCheck />}{ticket.assignedToUserId ? "Reassign" : "Assign"}</Button></div>
    {agents.isError && <p className="text-sm text-destructive">Unable to load support agents.</p>}
    <div className="rounded-xl border bg-muted/30 p-4"><div className="flex items-start justify-between gap-4"><div><p className="text-sm font-medium">Automatic assignment</p><p className="mt-1 text-xs leading-5 text-muted-foreground">Routes this ticket to the active support agent with the fewest unresolved tickets.</p></div><Button disabled={mutations.autoAssign.isPending} onClick={autoAssign} variant="outline">{mutations.autoAssign.isPending ? <LoaderCircle className="animate-spin" /> : <Bot />} Auto-assign</Button></div></div>
  </div>;
}
