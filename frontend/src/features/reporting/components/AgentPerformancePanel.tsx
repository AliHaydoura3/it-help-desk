import { CircleCheckBig, Gauge, Headphones, Timer } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useAgentPerformanceReport } from "../hooks/useReporting";
import type { DateRangeFilter } from "../types/reporting";
import { formatGeneratedAt, formatHours, formatPercentage } from "../utils/reportingPresentation";
import { HorizontalBarChart } from "./HorizontalBarChart";
import { MetricCard } from "./MetricCard";
import { ReportError, ReportLoading } from "./ReportQueryState";

export function AgentPerformancePanel({ filter }: { filter: DateRangeFilter }) {
  const query = useAgentPerformanceReport(filter);
  if (query.isLoading) return <ReportLoading />;
  if (query.isError || !query.data) return <ReportError onRetry={() => void query.refetch()} />;

  const agents = query.data.items;
  const totalResolved = agents.reduce((sum, agent) => sum + agent.resolvedTickets, 0);
  const totalActive = agents.reduce((sum, agent) => sum + agent.activeAssignedTickets, 0);
  const complianceValues = agents.flatMap((agent) => agent.slaCompliancePercentage === null ? [] : [agent.slaCompliancePercentage]);
  const averageCompliance = complianceValues.length === 0 ? null : complianceValues.reduce((sum, value) => sum + value, 0) / complianceValues.length;

  return (
    <div className="space-y-6">
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <MetricCard icon={Headphones} label="Active agents" value={agents.length} />
        <MetricCard icon={CircleCheckBig} label="Resolved tickets" value={totalResolved} tone="success" />
        <MetricCard icon={Timer} label="Assigned workload" value={totalActive} />
        <MetricCard icon={Gauge} label="Avg. agent compliance" value={formatPercentage(averageCompliance)} />
      </div>
      <Card className="shadow-sm">
        <CardHeader><CardTitle>Resolved tickets by agent</CardTitle></CardHeader>
        <CardContent>
          {agents.length === 0 ? <Empty /> : <HorizontalBarChart items={agents.map((agent) => ({ label: `${agent.firstName} ${agent.lastName}`, value: agent.resolvedTickets }))} />}
        </CardContent>
      </Card>
      <Card className="gap-0 py-0 shadow-sm">
        <div className="overflow-x-auto">
          <table className="w-full min-w-4xl text-left text-sm">
            <thead className="border-b bg-muted/40 text-xs uppercase tracking-wide text-muted-foreground">
              <tr><th className="px-5 py-3">Agent</th><th className="px-5 py-3 text-right">Active</th><th className="px-5 py-3 text-right">Pending</th><th className="px-5 py-3 text-right">Resolved</th><th className="px-5 py-3 text-right">Avg. resolution</th><th className="px-5 py-3 text-right">SLA</th></tr>
            </thead>
            <tbody className="divide-y">
              {agents.map((agent) => (
                <tr key={agent.agentId} className="hover:bg-muted/25">
                  <td className="px-5 py-4"><p className="font-medium">{agent.firstName} {agent.lastName}</p><p className="text-xs text-muted-foreground">{agent.email}</p></td>
                  <NumberCell value={agent.activeAssignedTickets} />
                  <NumberCell value={agent.pendingTickets} />
                  <NumberCell value={agent.resolvedTickets} />
                  <td className="px-5 py-4 text-right tabular-nums">{formatHours(agent.averageResolutionHours)}</td>
                  <td className="px-5 py-4 text-right"><ComplianceBadge value={agent.slaCompliancePercentage} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        {agents.length === 0 && <Empty />}
      </Card>
      <p className="text-right text-[11px] text-muted-foreground">Updated {formatGeneratedAt(query.data.generatedAtUtc)}</p>
    </div>
  );
}

function NumberCell({ value }: { value: number }) {
  return <td className="px-5 py-4 text-right font-medium tabular-nums">{value}</td>;
}

function ComplianceBadge({ value }: { value: number | null }) {
  const className = value === null ? "bg-muted text-muted-foreground" : value >= 90 ? "bg-emerald-500/10 text-emerald-700" : value >= 75 ? "bg-amber-500/10 text-amber-700" : "bg-destructive/10 text-destructive";
  return <span className={`inline-flex rounded-full px-2 py-1 text-xs font-medium ${className}`}>{formatPercentage(value)}</span>;
}

function Empty() {
  return <div className="py-14 text-center text-sm text-muted-foreground">No agent performance data for this period.</div>;
}
