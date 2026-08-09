import type { NotificationFilters } from "../types/communication";

export const communicationKeys = {
  all: ["communication"] as const,
  comments: (ticketId: string) => ["communication", "tickets", ticketId, "comments"] as const,
  commentsPage: (ticketId: string, pageNumber: number, pageSize: number) =>
    ["communication", "tickets", ticketId, "comments", { pageNumber, pageSize }] as const,
  mentions: (ticketId: string, search: string) =>
    ["communication", "tickets", ticketId, "mentionable-agents", search] as const,
  notifications: ["communication", "notifications"] as const,
  notificationsPage: (filters: NotificationFilters) =>
    ["communication", "notifications", "list", filters] as const,
  unreadCount: ["communication", "notifications", "unread-count"] as const,
};
