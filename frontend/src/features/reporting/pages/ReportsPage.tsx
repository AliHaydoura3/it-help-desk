import { useMemo, useState } from "react";
import {
  Activity,
  BarChart3,
  FileChartColumn,
  Gauge,
  Headphones,
  LogOut,
  ShieldCheck,
  TicketCheck,
  UserRound,
  Users,
} from "lucide-react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import { logoutSession } from "@/features/auth/api/logout";
import { ROLE_LABELS, type UserRole } from "@/features/auth/authorization/roles";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { NotificationBell } from "@/features/communication/components/NotificationBell";
import { getApiErrorMessage } from "@/features/users/utils/getApiErrorMessage";
import { AgentPerformancePanel } from "../components/AgentPerformancePanel";
import { EmployeeActivityPanel } from "../components/EmployeeActivityPanel";
import { ExportButtons } from "../components/ExportButtons";
import { MonthlyReportPanel } from "../components/MonthlyReportPanel";
import { OverviewReportPanel } from "../components/OverviewReportPanel";
import { ReportFilters } from "../components/ReportFilters";
import { SlaReportPanel } from "../components/SlaReportPanel";
import { useExportReport } from "../hooks/useReporting";
import type {
  ReportExportFormat,
  ReportType,
} from "../types/reporting";
import {
  getDefaultReportDates,
  saveReportDownload,
  toReportDateRange,
} from "../utils/reportingPresentation";

const TABS = [
  { value: "overview", label: "Overview", icon: BarChart3, reportType: "Dashboard" },
  { value: "agents", label: "Agents", icon: Headphones, reportType: "AgentPerformance" },
  { value: "monthly", label: "Monthly", icon: FileChartColumn, reportType: "MonthlyTickets" },
  { value: "sla", label: "SLA", icon: Gauge, reportType: "Sla" },
  { value: "activity", label: "User activity", icon: Activity, reportType: "EmployeeActivity" },
] as const satisfies readonly {
  value: string;
  label: string;
  icon: typeof BarChart3;
  reportType: ReportType;
}[];

type ReportingTab = (typeof TABS)[number]["value"];

export default function ReportsPage() {
  const auth = useAuth();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const defaults = useMemo(getDefaultReportDates, []);
  const [from, setFrom] = useState(defaults.from);
  const [to, setTo] = useState(defaults.to);
  const [months, setMonths] = useState(12);
  const [activityRole, setActivityRole] = useState<UserRole | undefined>();
  const [pendingFormat, setPendingFormat] = useState<ReportExportFormat | null>(null);
  const exportMutation = useExportReport();
  const requestedTab = searchParams.get("view");
  const activeTab: ReportingTab = isReportingTab(requestedTab) ? requestedTab : "overview";
  const range = useMemo(() => toReportDateRange(from, to), [from, to]);

  function changeTab(tab: ReportingTab) {
    const next = new URLSearchParams(searchParams);
    if (tab === "overview") next.delete("view");
    else next.set("view", tab);
    setSearchParams(next, { replace: true });
  }

  async function download(format: ReportExportFormat) {
    setPendingFormat(format);
    const reportType = TABS.find((tab) => tab.value === activeTab)?.reportType ?? "Dashboard";
    try {
      const result = await exportMutation.mutateAsync({
        type: reportType,
        format,
        ...(activeTab === "monthly" ? { months } : range),
        ...(activeTab === "activity" && activityRole ? { role: activityRole } : {}),
      });
      saveReportDownload(result);
      toast.success(`${format === "Pdf" ? "PDF" : "Excel"} report downloaded.`);
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Unable to export this report."));
    } finally {
      setPendingFormat(null);
    }
  }

  async function logout() {
    try { await logoutSession(); } catch { /* local logout continues */ }
    auth.logout();
    navigate("/login");
  }

  return (
    <div className="min-h-screen bg-muted/35">
      <header className="border-b bg-card">
        <div className="mx-auto flex h-16 max-w-7xl items-center gap-3 px-4 sm:px-6">
          <div className="flex size-9 items-center justify-center rounded-xl bg-primary text-primary-foreground"><ShieldCheck className="size-5" /></div>
          <div className="mr-auto"><p className="text-sm font-semibold">IT Help Desk</p><p className="text-xs text-muted-foreground">Reporting center</p></div>
          <Button aria-label="Tickets" render={<Link to="/tickets" />} variant="ghost"><TicketCheck /> <span className="hidden xl:inline">Tickets</span></Button>
          {auth.user?.role === "Admin" && <Button aria-label="Users" render={<Link to="/users" />} variant="ghost"><Users /> <span className="hidden xl:inline">Users</span></Button>}
          <NotificationBell />
          <Button render={<Link to="/profile" />} variant="ghost"><UserRound /> <span className="hidden sm:inline">Profile</span></Button>
          <Button aria-label="Sign out" onClick={logout} size="icon" variant="ghost"><LogOut /></Button>
        </div>
      </header>

      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6">
        <div className="flex flex-col justify-between gap-5 lg:flex-row lg:items-end">
          <div>
            <p className="text-sm font-medium text-muted-foreground">Management analytics</p>
            <h1 className="mt-1 text-3xl font-semibold tracking-tight">Dashboard & reports</h1>
            <p className="mt-2 max-w-2xl text-sm text-muted-foreground">Monitor ticket demand, support performance, SLA health, and user activity.</p>
          </div>
          <ExportButtons isPending={exportMutation.isPending} onExport={download} pendingFormat={pendingFormat} />
        </div>

        <div className="mt-7 overflow-x-auto border-b">
          <nav aria-label="Reporting sections" className="flex min-w-max gap-1">
            {TABS.map((tab) => {
              const Icon = tab.icon;
              const selected = tab.value === activeTab;
              return <button aria-current={selected ? "page" : undefined} className={`flex items-center gap-2 border-b-2 px-3 py-3 text-sm font-medium transition-colors ${selected ? "border-foreground text-foreground" : "border-transparent text-muted-foreground hover:text-foreground"}`} key={tab.value} onClick={() => changeTab(tab.value)}><Icon className="size-4" />{tab.label}</button>;
            })}
          </nav>
        </div>

        {activeTab !== "monthly" && (
          <div className="mt-5">
            <ReportFilters from={from} onFromChange={setFrom} onReset={() => { setFrom(defaults.from); setTo(defaults.to); }} onToChange={setTo} to={to} />
          </div>
        )}

        <section className="mt-6" aria-label={`${TABS.find((tab) => tab.value === activeTab)?.label} report`}>
          {activeTab === "overview" && <OverviewReportPanel filter={range} />}
          {activeTab === "agents" && <AgentPerformancePanel filter={range} />}
          {activeTab === "monthly" && <MonthlyReportPanel months={months} onMonthsChange={setMonths} />}
          {activeTab === "sla" && <SlaReportPanel filter={range} />}
          {activeTab === "activity" && <EmployeeActivityPanel filter={range} onRoleChange={setActivityRole} role={activityRole} />}
        </section>

        <footer className="mt-10 border-t py-5 text-xs text-muted-foreground">
          Visible to {auth.user ? ROLE_LABELS[auth.user.role] : "authorized management"}. Metrics use UTC reporting periods.
        </footer>
      </main>
    </div>
  );
}

function isReportingTab(value: string | null): value is ReportingTab {
  return TABS.some((tab) => tab.value === value);
}
