import { apiClient } from "@/shared/api/apiClient";
import type {
  AgentPerformanceReport,
  DashboardReport,
  DateRangeFilter,
  EmployeeActivityFilter,
  EmployeeActivityReport,
  ExportReportRequest,
  MonthlyTicketReport,
  ReportDownload,
  SlaReport,
} from "../types/reporting";

export async function getDashboardReport(
  filter: DateRangeFilter,
): Promise<DashboardReport> {
  return (await apiClient.get<DashboardReport>("/reports/dashboard", {
    params: filter,
  })).data;
}

export async function getAgentPerformanceReport(
  filter: DateRangeFilter,
): Promise<AgentPerformanceReport> {
  return (await apiClient.get<AgentPerformanceReport>(
    "/reports/agent-performance",
    { params: filter },
  )).data;
}

export async function getMonthlyTicketReport(
  months: number,
): Promise<MonthlyTicketReport> {
  return (await apiClient.get<MonthlyTicketReport>("/reports/monthly", {
    params: { months },
  })).data;
}

export async function getSlaReport(
  filter: DateRangeFilter,
): Promise<SlaReport> {
  return (await apiClient.get<SlaReport>("/reports/sla", {
    params: filter,
  })).data;
}

export async function getEmployeeActivityReport(
  filter: EmployeeActivityFilter,
): Promise<EmployeeActivityReport> {
  return (await apiClient.get<EmployeeActivityReport>(
    "/reports/employee-activity",
    { params: filter },
  )).data;
}

export async function exportReport(
  request: ExportReportRequest,
): Promise<ReportDownload> {
  const response = await apiClient.get<Blob>("/reports/export", {
    params: request,
    responseType: "blob",
  });

  return {
    blob: response.data,
    fileName: getFileName(
      response.headers["content-disposition"],
      `${request.type.toLowerCase()}-report.${request.format === "Pdf" ? "pdf" : "xlsx"}`,
    ),
  };
}

function getFileName(contentDisposition: unknown, fallback: string): string {
  if (typeof contentDisposition !== "string") return fallback;
  const encoded = contentDisposition.match(/filename\*=UTF-8''([^;]+)/i)?.[1];
  if (encoded) return decodeURIComponent(encoded.replaceAll('"', ""));
  return contentDisposition.match(/filename="?([^";]+)"?/i)?.[1] ?? fallback;
}
