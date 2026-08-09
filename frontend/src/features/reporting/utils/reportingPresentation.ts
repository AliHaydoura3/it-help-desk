import type { DateRangeFilter, ReportDownload } from "../types/reporting";

export function toReportDateRange(from: string, to: string): DateRangeFilter {
  return {
    fromUtc: from ? `${from}T00:00:00.000Z` : undefined,
    toUtc: to ? `${to}T23:59:59.999Z` : undefined,
  };
}

export function getDefaultReportDates(): { from: string; to: string } {
  const today = new Date();
  const from = new Date(today);
  from.setDate(from.getDate() - 29);
  return { from: toDateInput(from), to: toDateInput(today) };
}

export function formatHours(value: number | null): string {
  if (value === null) return "—";
  if (value < 1) return `${Math.round(value * 60)}m`;
  return `${value.toLocaleString(undefined, { maximumFractionDigits: 1 })}h`;
}

export function formatPercentage(value: number | null): string {
  return value === null
    ? "—"
    : `${value.toLocaleString(undefined, { maximumFractionDigits: 1 })}%`;
}

export function formatGeneratedAt(value: string): string {
  return new Date(value).toLocaleString(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  });
}

export function saveReportDownload(download: ReportDownload): void {
  const url = URL.createObjectURL(download.blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = download.fileName;
  document.body.append(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

function toDateInput(value: Date): string {
  const year = value.getFullYear();
  const month = String(value.getMonth() + 1).padStart(2, "0");
  const day = String(value.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}
