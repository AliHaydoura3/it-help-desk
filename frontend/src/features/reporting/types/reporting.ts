import type { UserRole } from "@/features/auth/authorization/roles";
import type {
  TicketCategory,
  TicketPriority,
} from "@/features/tickets/types/ticket";

export interface ReportingPeriod {
  fromUtc: string;
  toUtc: string;
}

export interface DateRangeFilter {
  fromUtc?: string;
  toUtc?: string;
}

export interface CategoryMetric {
  category: TicketCategory;
  count: number;
}

export interface PriorityMetric {
  priority: TicketPriority;
  count: number;
}

export interface AgentPerformanceItem {
  agentId: string;
  firstName: string;
  lastName: string;
  email: string;
  activeAssignedTickets: number;
  pendingTickets: number;
  resolvedTickets: number;
  averageResolutionHours: number | null;
  slaCompliancePercentage: number | null;
}

export interface SlaSummary {
  evaluatedTickets: number;
  compliantTickets: number;
  breachedTickets: number;
  activeAtRiskTickets: number;
  activeBreachedTickets: number;
  compliancePercentage: number | null;
}

export interface DashboardReport {
  generatedAtUtc: string;
  performancePeriod: ReportingPeriod;
  totalTickets: number;
  openTickets: number;
  inProgressTickets: number;
  pendingTickets: number;
  resolvedTickets: number;
  closedTickets: number;
  ticketsByCategory: CategoryMetric[];
  ticketsByPriority: PriorityMetric[];
  agentPerformance: AgentPerformanceItem[];
  averageResolutionHours: number | null;
  sla: SlaSummary;
}

export interface AgentPerformanceReport {
  generatedAtUtc: string;
  period: ReportingPeriod;
  items: AgentPerformanceItem[];
}

export interface MonthlyTicketMetric {
  year: number;
  month: number;
  label: string;
  createdTickets: number;
  resolvedTickets: number;
  closedTickets: number;
  cancelledTickets: number;
  averageResolutionHours: number | null;
}

export interface MonthlyTicketReport {
  generatedAtUtc: string;
  period: ReportingPeriod;
  totalCreatedTickets: number;
  totalResolvedTickets: number;
  totalClosedTickets: number;
  totalCancelledTickets: number;
  averageResolutionHours: number | null;
  months: MonthlyTicketMetric[];
}

export interface SlaPriorityMetric {
  priority: TicketPriority;
  targetHours: number;
  evaluatedTickets: number;
  compliantTickets: number;
  breachedTickets: number;
  activeAtRiskTickets: number;
  activeBreachedTickets: number;
  compliancePercentage: number | null;
  averageResolutionHours: number | null;
}

export interface SlaReport {
  generatedAtUtc: string;
  period: ReportingPeriod;
  summary: SlaSummary;
  byPriority: SlaPriorityMetric[];
}

export interface EmployeeActivityItem {
  userId: string;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
  isActive: boolean;
  ticketsCreated: number;
  ticketsResolved: number;
  commentsAdded: number;
  successfulActions: number;
  failedActions: number;
  lastActivityAtUtc: string | null;
}

export interface EmployeeActivityReport {
  generatedAtUtc: string;
  period: ReportingPeriod;
  items: EmployeeActivityItem[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface EmployeeActivityFilter extends DateRangeFilter {
  role?: UserRole;
  pageNumber: number;
  pageSize: number;
}

export type ReportType =
  | "Dashboard"
  | "MonthlyTickets"
  | "AgentPerformance"
  | "Sla"
  | "EmployeeActivity";

export type ReportExportFormat = "Pdf" | "Excel";

export interface ExportReportRequest extends DateRangeFilter {
  type: ReportType;
  format: ReportExportFormat;
  months?: number;
  role?: UserRole;
}

export interface ReportDownload {
  blob: Blob;
  fileName: string;
}
