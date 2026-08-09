import { apiClient } from "@/shared/api/apiClient";
import type {
  AddTicketCommentInput,
  MarkAllNotificationsReadResponse,
  MentionableAgent,
  Notification,
  NotificationFilters,
  NotificationsResponse,
  TicketComment,
  TicketCommentsResponse,
  UnreadNotificationCountResponse,
} from "../types/communication";

export async function getTicketComments(
  ticketId: string,
  pageNumber: number,
  pageSize: number,
): Promise<TicketCommentsResponse> {
  return (
    await apiClient.get<TicketCommentsResponse>(`/tickets/${ticketId}/comments`, {
      params: { pageNumber, pageSize },
    })
  ).data;
}

export async function addTicketComment(
  ticketId: string,
  input: AddTicketCommentInput,
): Promise<TicketComment> {
  return (await apiClient.post<TicketComment>(`/tickets/${ticketId}/comments`, input)).data;
}

export async function getMentionableAgents(
  ticketId: string,
  search?: string,
  limit = 20,
): Promise<MentionableAgent[]> {
  return (
    await apiClient.get<MentionableAgent[]>(`/tickets/${ticketId}/mentionable-agents`, {
      params: { search, limit },
    })
  ).data;
}

export async function getNotifications(filters: NotificationFilters): Promise<NotificationsResponse> {
  return (await apiClient.get<NotificationsResponse>("/notifications", { params: filters })).data;
}

export async function getUnreadNotificationCount(): Promise<UnreadNotificationCountResponse> {
  return (await apiClient.get<UnreadNotificationCountResponse>("/notifications/unread-count")).data;
}

export async function markNotificationRead(notificationId: string): Promise<Notification> {
  return (await apiClient.patch<Notification>(`/notifications/${notificationId}/read`)).data;
}

export async function markAllNotificationsRead(): Promise<MarkAllNotificationsReadResponse> {
  return (await apiClient.post<MarkAllNotificationsReadResponse>("/notifications/read-all")).data;
}
