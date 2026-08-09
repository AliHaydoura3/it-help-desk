import type {
  DateRangeFilter,
  EmployeeActivityFilter,
} from "../types/reporting";

const root = ["reporting"] as const;

export const reportingKeys = {
  all: root,
  dashboard: (filter: DateRangeFilter) => [...root, "dashboard", filter] as const,
  agents: (filter: DateRangeFilter) => [...root, "agents", filter] as const,
  monthly: (months: number) => [...root, "monthly", months] as const,
  sla: (filter: DateRangeFilter) => [...root, "sla", filter] as const,
  activity: (filter: EmployeeActivityFilter) => [...root, "activity", filter] as const,
};
