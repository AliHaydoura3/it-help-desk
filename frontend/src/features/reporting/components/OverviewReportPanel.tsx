import {
  CircleCheckBig,
  CircleDot,
  Clock3,
  Gauge,
  Timer,
  Tickets,
} from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type { DateRangeFilter } from "../types/reporting";
import { formatGeneratedAt, formatHours, formatPercentage } from "../utils/reportingPresentation";
import { HorizontalBarChart } from "./HorizontalBarChart";
import { MetricCard } from "./MetricCard";
import { ReportError, ReportLoading } from "./ReportQueryState";
import { useDashboardReport } from "../hooks/useReporting";

export function OverviewReportPanel({ filter }: { filter: DateRangeFilter }) {
  const query = useDashboardReport(filter);

  if (query.isLoading) return <ReportLoading />;
  if (query.isError || !query.data) return <ReportError onRetry={() => void query.refetch()} />;

  const report = query.data;
  return (
    <div className="space-y-6">
      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-5">
        <MetricCard icon={Tickets} label="Total tickets" value={report.totalTickets} detail="Active dataset" />
        <MetricCard icon={CircleDot} label="Open" value={report.openTickets} />
        <MetricCard icon={Clock3} label="Pending" value={report.pendingTickets} tone="warning" />
        <MetricCard icon={CircleCheckBig} label="Resolved" value={report.resolvedTickets} tone="success" />
        <MetricCard icon={Timer} label="Avg. resolution" value={formatHours(report.averageResolutionHours)} detail="Selected period" />
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <ChartCard title="Tickets by category">
          <HorizontalBarChart items={report.ticketsByCategory.map((item, index) => ({
            label: splitLabel(item.category),
            value: item.count,
            tone: index % 3 === 0 ? "dark" : index % 3 === 1 ? "mid" : "light",
          }))} />
        </ChartCard>
        <ChartCard title="Tickets by priority">
          <HorizontalBarChart items={report.ticketsByPriority.map((item) => ({
            label: item.priority,
            value: item.count,
            tone: item.priority === "Critical" ? "danger" : item.priority === "High" ? "dark" : "mid",
          }))} />
        </ChartCard>
      </div>

      <div className="grid gap-6 lg:grid-cols-[1.2fr_0.8fr]">
        <ChartCard title="Agent resolution volume">
          {report.agentPerformance.length === 0 ? (
            <EmptyMessage>No active support agents found.</EmptyMessage>
          ) : (
            <HorizontalBarChart items={report.agentPerformance.map((agent) => ({
              label: `${agent.firstName} ${agent.lastName}`,
              value: agent.resolvedTickets,
            }))} />
          )}
        </ChartCard>
        <Card className="shadow-sm">
          <CardHeader>
            <CardTitle className="flex items-center gap-2"><Gauge className="size-4" /> SLA health</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-end justify-between gap-4 border-b pb-5">
              <div>
                <p className="text-3xl font-semibold tracking-tight">{formatPercentage(report.sla.compliancePercentage)}</p>
                <p className="mt-1 text-xs text-muted-foreground">Resolution compliance</p>
              </div>
              <p className="text-right text-xs text-muted-foreground">{report.sla.compliantTickets} of {report.sla.evaluatedTickets}<br />evaluated tickets</p>
            </div>
            <dl className="mt-4 grid grid-cols-3 gap-3 text-center">
              <SlaDatum label="Breached" value={report.sla.breachedTickets} tone="text-destructive" />
              <SlaDatum label="At risk" value={report.sla.activeAtRiskTickets} tone="text-amber-600" />
              <SlaDatum label="Active breach" value={report.sla.activeBreachedTickets} tone="text-destructive" />
            </dl>
          </CardContent>
        </Card>
      </div>
      <GeneratedAt value={report.generatedAtUtc} />
    </div>
  );
}

function ChartCard({ title, children }: { title: string; children: React.ReactNode }) {
  return <Card className="shadow-sm"><CardHeader><CardTitle>{title}</CardTitle></CardHeader><CardContent>{children}</CardContent></Card>;
}

function SlaDatum({ label, value, tone }: { label: string; value: number; tone: string }) {
  return <div><dd className={`text-xl font-semibold ${tone}`}>{value}</dd><dt className="mt-1 text-[11px] text-muted-foreground">{label}</dt></div>;
}

function EmptyMessage({ children }: { children: React.ReactNode }) {
  return <div className="py-12 text-center text-sm text-muted-foreground">{children}</div>;
}

function GeneratedAt({ value }: { value: string }) {
  return <p className="text-right text-[11px] text-muted-foreground">Updated {formatGeneratedAt(value)}</p>;
}

function splitLabel(value: string): string {
  return value.replace(/([a-z])([A-Z])/g, "$1 $2");
}
