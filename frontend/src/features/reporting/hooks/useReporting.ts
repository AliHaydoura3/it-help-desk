import { useMutation, useQuery } from "@tanstack/react-query";

import {
  exportReport,
  getAgentPerformanceReport,
  getDashboardReport,
  getEmployeeActivityReport,
  getMonthlyTicketReport,
  getSlaReport,
} from "../api/reporting";
import type {
  DateRangeFilter,
  EmployeeActivityFilter,
} from "../types/reporting";
import { reportingKeys } from "./reportingKeys";

export function useDashboardReport(filter: DateRangeFilter, enabled = true) {
  return useQuery({
    queryKey: reportingKeys.dashboard(filter),
    queryFn: () => getDashboardReport(filter),
    enabled,
  });
}

export function useAgentPerformanceReport(filter: DateRangeFilter, enabled = true) {
  return useQuery({
    queryKey: reportingKeys.agents(filter),
    queryFn: () => getAgentPerformanceReport(filter),
    enabled,
  });
}

export function useMonthlyTicketReport(months: number, enabled = true) {
  return useQuery({
    queryKey: reportingKeys.monthly(months),
    queryFn: () => getMonthlyTicketReport(months),
    enabled,
  });
}

export function useSlaReport(filter: DateRangeFilter, enabled = true) {
  return useQuery({
    queryKey: reportingKeys.sla(filter),
    queryFn: () => getSlaReport(filter),
    enabled,
  });
}

export function useEmployeeActivityReport(
  filter: EmployeeActivityFilter,
  enabled = true,
) {
  return useQuery({
    queryKey: reportingKeys.activity(filter),
    queryFn: () => getEmployeeActivityReport(filter),
    placeholderData: (previous) => previous,
    enabled,
  });
}

export function useExportReport() {
  return useMutation({ mutationFn: exportReport });
}
