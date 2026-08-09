import { Ban, CalendarRange, CircleCheckBig, Timer } from "lucide-react";

import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { useMonthlyTicketReport } from "../hooks/useReporting";
import { formatGeneratedAt, formatHours } from "../utils/reportingPresentation";
import { MetricCard } from "./MetricCard";
import { MonthlyTrendChart } from "./MonthlyTrendChart";
import { ReportError, ReportLoading } from "./ReportQueryState";

interface MonthlyReportPanelProps {
  months: number;
  onMonthsChange: (months: number) => void;
}

export function MonthlyReportPanel({ months, onMonthsChange }: MonthlyReportPanelProps) {
  const query = useMonthlyTicketReport(months);
  if (query.isLoading) return <ReportLoading />;
  if (query.isError || !query.data) return <ReportError onRetry={() => void query.refetch()} />;
  const report = query.data;

  return (
    <div className="space-y-6">
      <div className="flex justify-end">
        <label className="flex items-center gap-2 text-xs text-muted-foreground">
          Period
          <select className="h-9 rounded-lg border bg-background px-3 text-sm text-foreground" onChange={(event) => onMonthsChange(Number(event.target.value))} value={months}>
            <option value={6}>Last 6 months</option><option value={12}>Last 12 months</option><option value={24}>Last 24 months</option><option value={36}>Last 36 months</option>
          </select>
        </label>
      </div>
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <MetricCard icon={CalendarRange} label="Created" value={report.totalCreatedTickets} />
        <MetricCard icon={CircleCheckBig} label="Resolved" value={report.totalResolvedTickets} tone="success" />
        <MetricCard icon={Ban} label="Cancelled" value={report.totalCancelledTickets} tone="danger" />
        <MetricCard icon={Timer} label="Avg. resolution" value={formatHours(report.averageResolutionHours)} />
      </div>
      <Card className="shadow-sm"><CardHeader><CardTitle>Ticket volume trend</CardTitle></CardHeader><div className="px-4 pb-4"><MonthlyTrendChart months={report.months} /></div></Card>
      <Card className="gap-0 py-0 shadow-sm">
        <div className="overflow-x-auto"><table className="w-full min-w-3xl text-left text-sm"><thead className="border-b bg-muted/40 text-xs uppercase tracking-wide text-muted-foreground"><tr><th className="px-5 py-3">Month</th><th className="px-5 py-3 text-right">Created</th><th className="px-5 py-3 text-right">Resolved</th><th className="px-5 py-3 text-right">Closed</th><th className="px-5 py-3 text-right">Cancelled</th><th className="px-5 py-3 text-right">Avg. resolution</th></tr></thead><tbody className="divide-y">{report.months.map((month) => <tr key={`${month.year}-${month.month}`} className="hover:bg-muted/25"><td className="px-5 py-4 font-medium">{month.label}</td><NumberCell value={month.createdTickets} /><NumberCell value={month.resolvedTickets} /><NumberCell value={month.closedTickets} /><NumberCell value={month.cancelledTickets} /><td className="px-5 py-4 text-right tabular-nums">{formatHours(month.averageResolutionHours)}</td></tr>)}</tbody></table></div>
      </Card>
      <p className="text-right text-[11px] text-muted-foreground">Updated {formatGeneratedAt(report.generatedAtUtc)}</p>
    </div>
  );
}

function NumberCell({ value }: { value: number }) {
  return <td className="px-5 py-4 text-right font-medium tabular-nums">{value}</td>;
}
