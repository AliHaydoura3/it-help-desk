import type { TicketCategory } from "@/features/tickets/types/ticket";

export interface AdminDashboard {
  users: { total: number; active: number; inactive: number; supportAgents: number };
  tickets: {
    total: number; open: number; inProgress: number; pending: number;
    resolved: number; closed: number; critical: number; unassigned: number;
  };
  notifications: { total: number; pendingEmail: number; failedEmail: number; unread: number };
  storage: { attachmentCount: number; attachmentBytes: number };
  audit: { requestsLast24Hours: number; failedRequestsLast24Hours: number };
  generatedAtUtc: string;
}

export interface AdminRole {
  name: string;
  displayName: string;
  description: string;
  assignedUserCount: number;
  permissions: string[];
}

export interface TicketCategorySetting {
  category: TicketCategory;
  displayName: string;
  description: string;
  isActive: boolean;
  sortOrder: number;
  updatedAtUtc: string;
  updatedByUserId: string | null;
}

export type UpdateTicketCategoryRequest = Pick<
  TicketCategorySetting,
  "category" | "displayName" | "description" | "isActive" | "sortOrder"
>;

export interface SystemSettings {
  organizationName: string;
  supportEmail: string;
  automaticAssignmentEnabled: boolean;
  emailNotificationsEnabled: boolean;
  maximumOpenTicketsPerEmployee: number;
  updatedAtUtc: string;
  updatedByUserId: string | null;
}

export type UpdateSystemSettingsRequest = Omit<
  SystemSettings,
  "updatedAtUtc" | "updatedByUserId"
>;
