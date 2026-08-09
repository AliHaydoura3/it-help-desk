import { Activity, AlertTriangle, Bell, Database, HardDrive, RefreshCw, TicketCheck, Users } from "lucide-react";
import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useAdminDashboard } from "../hooks/useAdmin";

export function AdminOverview() {
  const query = useAdminDashboard();
  if (query.isLoading) return <PanelState title="Loading system health…" />;
  if (query.isError || !query.data) return <PanelState title="System health could not be loaded" action={<Button onClick={() => query.refetch()} variant="outline"><RefreshCw />Try again</Button>} />;
  const data = query.data;
  const ticketRows = [
    ["Open", data.tickets.open], ["In progress", data.tickets.inProgress], ["Pending", data.tickets.pending],
    ["Resolved", data.tickets.resolved], ["Closed", data.tickets.closed],
  ] as const;
  const maxTickets = Math.max(...ticketRows.map(([, count]) => count), 1);

  return <>
    <PageHeading eyebrow="Operations" title="System monitoring" description="A live operational view of access, ticket workload, delivery health, storage, and API activity." action={<Button onClick={() => query.refetch()} variant="outline"><RefreshCw />Refresh</Button>} />
    <div className="mt-7 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
      <Metric icon={Users} label="Active users" value={data.users.active} note={`${data.users.inactive} inactive`} />
      <Metric icon={TicketCheck} label="Active tickets" value={data.tickets.open + data.tickets.inProgress + data.tickets.pending} note={`${data.tickets.unassigned} unassigned`} />
      <Metric icon={Bell} label="Unread alerts" value={data.notifications.unread} note={`${data.notifications.failedEmail} email failures`} danger={data.notifications.failedEmail > 0} />
      <Metric icon={Activity} label="Requests (24h)" value={data.audit.requestsLast24Hours} note={`${data.audit.failedRequestsLast24Hours} failed`} danger={data.audit.failedRequestsLast24Hours > 0} />
    </div>
    <div className="mt-6 grid gap-6 xl:grid-cols-[1.35fr_1fr]">
      <Card><CardContent className="p-5"><div className="flex items-center justify-between"><div><h2 className="font-semibold">Ticket workload</h2><p className="mt-1 text-sm text-muted-foreground">Current non-cancelled ticket distribution.</p></div><span className="rounded-lg bg-muted px-2.5 py-1 text-xs font-medium">{data.tickets.total} total</span></div><div className="mt-6 space-y-4">{ticketRows.map(([label, count]) => <div key={label}><div className="mb-1.5 flex justify-between text-sm"><span>{label}</span><span className="font-medium">{count}</span></div><div className="h-2 overflow-hidden rounded-full bg-muted"><div className="h-full rounded-full bg-primary" style={{ width: `${Math.max((count / maxTickets) * 100, count ? 4 : 0)}%` }} /></div></div>)}</div></CardContent></Card>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-1">
        <HealthCard icon={HardDrive} title="Secure attachment storage" rows={[["Files", data.storage.attachmentCount.toLocaleString()], ["Storage used", formatBytes(data.storage.attachmentBytes)]]} />
        <HealthCard icon={Database} title="Notification delivery" rows={[["Queued emails", data.notifications.pendingEmail.toLocaleString()], ["Failed emails", data.notifications.failedEmail.toLocaleString()]]} warning={data.notifications.failedEmail > 0} />
      </div>
    </div>
    <div className="mt-6 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
      <QuickLink to="/users" title="Manage users" text="Accounts, status, and role assignment" />
      <QuickLink to="/admin/categories" title="Configure categories" text="Availability and ticket labels" />
      <QuickLink to="/reports" title="Generate reports" text="PDF and Excel exports" />
      <QuickLink to="/activity-logs" title="Review audit trail" text="Administrative and API activity" />
    </div>
    <p className="mt-5 text-xs text-muted-foreground">Last calculated {new Date(data.generatedAtUtc).toLocaleString()}.</p>
  </>;
}

export function PageHeading({ eyebrow, title, description, action }: { eyebrow: string; title: string; description: string; action?: React.ReactNode }) {
  return <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end"><div><p className="text-sm font-medium text-muted-foreground">{eyebrow}</p><h1 className="mt-1 text-2xl font-semibold tracking-tight sm:text-3xl">{title}</h1><p className="mt-2 max-w-2xl text-sm text-muted-foreground">{description}</p></div>{action}</div>;
}

function Metric({ icon: Icon, label, value, note, danger }: { icon: typeof Users; label: string; value: number; note: string; danger?: boolean }) { return <Card><CardContent className="flex items-center gap-4 p-5"><div className={`flex size-11 items-center justify-center rounded-xl ${danger ? "bg-destructive/10 text-destructive" : "bg-primary/10 text-primary"}`}><Icon className="size-5" /></div><div><p className="text-xs text-muted-foreground">{label}</p><p className="text-2xl font-semibold">{value.toLocaleString()}</p><p className={`text-xs ${danger ? "text-destructive" : "text-muted-foreground"}`}>{note}</p></div></CardContent></Card>; }
function HealthCard({ icon: Icon, title, rows, warning }: { icon: typeof Database; title: string; rows: readonly (readonly [string, string])[]; warning?: boolean }) { return <Card><CardContent className="p-5"><div className="flex items-center gap-3"><div className={`flex size-9 items-center justify-center rounded-lg ${warning ? "bg-destructive/10 text-destructive" : "bg-muted text-muted-foreground"}`}>{warning ? <AlertTriangle className="size-4" /> : <Icon className="size-4" />}</div><h2 className="font-semibold">{title}</h2></div><div className="mt-4 divide-y">{rows.map(([label, value]) => <div className="flex justify-between py-2.5 text-sm" key={label}><span className="text-muted-foreground">{label}</span><span className="font-medium">{value}</span></div>)}</div></CardContent></Card>; }
function QuickLink({ to, title, text }: { to: string; title: string; text: string }) { return <Link className="rounded-xl border bg-card p-4 transition-colors hover:border-primary/50 hover:bg-primary/[0.03]" to={to}><p className="text-sm font-medium">{title}</p><p className="mt-1 text-xs text-muted-foreground">{text}</p></Link>; }
function PanelState({ title, action }: { title: string; action?: React.ReactNode }) { return <div className="flex min-h-96 flex-col items-center justify-center gap-4 text-center"><Activity className="size-8 text-muted-foreground" /><p className="font-medium">{title}</p>{action}</div>; }
function formatBytes(bytes: number) { if (bytes < 1024) return `${bytes} B`; const units = ["KB", "MB", "GB", "TB"]; let value = bytes / 1024; let index = 0; while (value >= 1024 && index < units.length - 1) { value /= 1024; index++; } return `${value.toFixed(value >= 10 ? 1 : 2)} ${units[index]}`; }
