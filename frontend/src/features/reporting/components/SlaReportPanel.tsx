import { CircleAlert, CircleCheckBig, Gauge, ShieldAlert } from "lucide-react";

import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { useSlaReport } from "../hooks/useReporting";
import type { DateRangeFilter } from "../types/reporting";
import { formatGeneratedAt, formatHours, formatPercentage } from "../utils/reportingPresentation";
import { HorizontalBarChart } from "./HorizontalBarChart";
import { MetricCard } from "./MetricCard";
import { ReportError, ReportLoading } from "./ReportQueryState";

export function SlaReportPanel({ filter }: { filter: DateRangeFilter }) {
  const query = useSlaReport(filter);
  if (query.isLoading) return <ReportLoading />;
  if (query.isError || !query.data) return <ReportError onRetry={() => void query.refetch()} />;
  const report = query.data;

  return (
    <div className="space-y-6">
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <MetricCard icon={Gauge} label="Compliance" value={formatPercentage(report.summary.compliancePercentage)} />
        <MetricCard icon={CircleCheckBig} label="Compliant" value={report.summary.compliantTickets} tone="success" />
        <MetricCard icon={CircleAlert} label="Active at risk" value={report.summary.activeAtRiskTickets} tone="warning" />
        <MetricCard icon={ShieldAlert} label="Total breaches" value={report.summary.breachedTickets + report.summary.activeBreachedTickets} tone="danger" />
      </div>
      <div className="grid gap-6 lg:grid-cols-2">
        <Card className="shadow-sm"><CardHeader><CardTitle>Compliance by priority</CardTitle></CardHeader><CardContent><HorizontalBarChart items={report.byPriority.map((metric) => ({ label: metric.priority, value: metric.compliancePercentage ?? 0, displayValue: formatPercentage(metric.compliancePercentage), tone: metric.compliancePercentage !== null && metric.compliancePercentage < 75 ? "danger" : "dark" }))} /></CardContent></Card>
        <Card className="shadow-sm"><CardHeader><CardTitle>Breaches by priority</CardTitle></CardHeader><CardContent><HorizontalBarChart items={report.byPriority.map((metric) => ({ label: metric.priority, value: metric.breachedTickets + metric.activeBreachedTickets, tone: "danger" }))} /></CardContent></Card>
      </div>
      <Card className="gap-0 py-0 shadow-sm">
        <div className="overflow-x-auto"><table className="w-full min-w-5xl text-left text-sm"><thead className="border-b bg-muted/40 text-xs uppercase tracking-wide text-muted-foreground"><tr><th className="px-5 py-3">Priority</th><th className="px-5 py-3 text-right">Target</th><th className="px-5 py-3 text-right">Evaluated</th><th className="px-5 py-3 text-right">Compliant</th><th className="px-5 py-3 text-right">Resolved breach</th><th className="px-5 py-3 text-right">At risk</th><th className="px-5 py-3 text-right">Active breach</th><th className="px-5 py-3 text-right">Avg. resolution</th></tr></thead><tbody className="divide-y">{report.byPriority.map((metric) => <tr key={metric.priority} className="hover:bg-muted/25"><td className="px-5 py-4"><p className="font-medium">{metric.priority}</p><p className="mt-0.5 text-xs text-muted-foreground">{formatPercentage(metric.compliancePercentage)} compliant</p></td><td className="px-5 py-4 text-right tabular-nums">{formatHours(metric.targetHours)}</td><NumberCell value={metric.evaluatedTickets} /><NumberCell value={metric.compliantTickets} /><NumberCell value={metric.breachedTickets} danger /><NumberCell value={metric.activeAtRiskTickets} warning /><NumberCell value={metric.activeBreachedTickets} danger /><td className="px-5 py-4 text-right tabular-nums">{formatHours(metric.averageResolutionHours)}</td></tr>)}</tbody></table></div>
      </Card>
      <p className="text-right text-[11px] text-muted-foreground">Updated {formatGeneratedAt(report.generatedAtUtc)}</p>
    </div>
  );
}

function NumberCell({ value, danger, warning }: { value: number; danger?: boolean; warning?: boolean }) {
  const tone = danger && value > 0 ? "text-destructive" : warning && value > 0 ? "text-amber-600" : "";
  return <td className={`px-5 py-4 text-right font-medium tabular-nums ${tone}`}>{value}</td>;
}
