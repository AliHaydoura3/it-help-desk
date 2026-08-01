import { apiClient } from "@/shared/api/apiClient";

export interface ActivityLog {
  id: string;
  userId: string | null;
  userEmail: string | null;
  action: string;
  resource: string;
  resourceId: string | null;
  ipAddress: string | null;
  succeeded: boolean;
  occurredAtUtc: string;
}

export interface ActivityLogsResponse {
  items: ActivityLog[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export async function getActivityLogs(pageNumber: number): Promise<ActivityLogsResponse> {
  return (await apiClient.get<ActivityLogsResponse>("/activity-logs", {
    params: { pageNumber, pageSize: 20 },
  })).data;
}
