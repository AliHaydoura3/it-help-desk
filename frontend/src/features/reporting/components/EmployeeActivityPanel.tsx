import { useEffect, useState } from "react";
import { Activity, MessageSquareText, TicketCheck, TicketPlus } from "lucide-react";

import { Card } from "@/components/ui/card";
import { ROLE_LABELS, USER_ROLES, type UserRole } from "@/features/auth/authorization/roles";
import { useEmployeeActivityReport } from "../hooks/useReporting";
import type { DateRangeFilter } from "../types/reporting";
import { formatGeneratedAt } from "../utils/reportingPresentation";
import { MetricCard } from "./MetricCard";
import { ReportError, ReportLoading } from "./ReportQueryState";
import { ReportingPagination } from "./ReportingPagination";

const PAGE_SIZE = 10;

interface EmployeeActivityPanelProps {
  filter: DateRangeFilter;
  role: UserRole | undefined;
  onRoleChange: (role: UserRole | undefined) => void;
}

export function EmployeeActivityPanel({ filter, role, onRoleChange }: EmployeeActivityPanelProps) {
  const [page, setPage] = useState(1);
  const query = useEmployeeActivityReport({ ...filter, role, pageNumber: page, pageSize: PAGE_SIZE });

  useEffect(() => setPage(1), [filter.fromUtc, filter.toUtc, role]);
  useEffect(() => {
    if (query.data && query.data.totalPages > 0 && page > query.data.totalPages) {
      setPage(query.data.totalPages);
    }
  }, [page, query.data]);

  if (query.isLoading) return <ReportLoading />;
  if (query.isError || !query.data) return <ReportError onRetry={() => void query.refetch()} />;
  const report = query.data;
  const totals = report.items.reduce((result, item) => ({
    created: result.created + item.ticketsCreated,
    resolved: result.resolved + item.ticketsResolved,
    comments: result.comments + item.commentsAdded,
    actions: result.actions + item.successfulActions + item.failedActions,
  }), { created: 0, resolved: 0, comments: 0, actions: 0 });

  return (
    <div className="space-y-6">
      <div className="flex justify-end">
        <label className="flex items-center gap-2 text-xs text-muted-foreground">
          Role
          <select className="h-9 rounded-lg border bg-background px-3 text-sm text-foreground" onChange={(event) => onRoleChange(event.target.value ? event.target.value as UserRole : undefined)} value={role ?? ""}>
            <option value="">All roles</option>
            {USER_ROLES.map((item) => <option key={item} value={item}>{ROLE_LABELS[item]}</option>)}
          </select>
        </label>
      </div>
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        <MetricCard icon={TicketPlus} label="Tickets created" value={totals.created} detail="Current page" />
        <MetricCard icon={TicketCheck} label="Tickets resolved" value={totals.resolved} detail="Current page" tone="success" />
        <MetricCard icon={MessageSquareText} label="Comments" value={totals.comments} detail="Current page" />
        <MetricCard icon={Activity} label="Audited actions" value={totals.actions} detail="Current page" />
      </div>
      <Card className="gap-0 py-0 shadow-sm">
        <div className="overflow-x-auto"><table className="w-full min-w-6xl text-left text-sm"><thead className="border-b bg-muted/40 text-xs uppercase tracking-wide text-muted-foreground"><tr><th className="px-5 py-3">User</th><th className="px-5 py-3">Role</th><th className="px-5 py-3 text-right">Created</th><th className="px-5 py-3 text-right">Resolved</th><th className="px-5 py-3 text-right">Comments</th><th className="px-5 py-3 text-right">Successful</th><th className="px-5 py-3 text-right">Failed</th><th className="px-5 py-3">Last activity</th></tr></thead><tbody className="divide-y">{report.items.map((item) => <tr key={item.userId} className="hover:bg-muted/25"><td className="px-5 py-4"><div className="flex items-center gap-3"><span className={`size-2 rounded-full ${item.isActive ? "bg-emerald-500" : "bg-muted-foreground/40"}`} /><div><p className="font-medium">{item.firstName} {item.lastName}</p><p className="text-xs text-muted-foreground">{item.email}</p></div></div></td><td className="px-5 py-4 text-xs text-muted-foreground">{ROLE_LABELS[item.role]}</td><NumberCell value={item.ticketsCreated} /><NumberCell value={item.ticketsResolved} /><NumberCell value={item.commentsAdded} /><NumberCell value={item.successfulActions} /><NumberCell value={item.failedActions} danger /><td className="whitespace-nowrap px-5 py-4 text-xs text-muted-foreground">{item.lastActivityAtUtc ? new Date(item.lastActivityAtUtc).toLocaleString() : "—"}</td></tr>)}</tbody></table></div>
        {report.items.length === 0 && <div className="py-16 text-center text-sm text-muted-foreground">No user activity matches these filters.</div>}
        <ReportingPagination disabled={query.isFetching} onPageChange={setPage} pageNumber={report.pageNumber} pageSize={report.pageSize} totalCount={report.totalCount} totalPages={report.totalPages} />
      </Card>
      <p className="text-right text-[11px] text-muted-foreground">Updated {formatGeneratedAt(report.generatedAtUtc)}</p>
    </div>
  );
}

function NumberCell({ value, danger }: { value: number; danger?: boolean }) {
  return <td className={`px-5 py-4 text-right font-medium tabular-nums ${danger && value > 0 ? "text-destructive" : ""}`}>{value}</td>;
}
