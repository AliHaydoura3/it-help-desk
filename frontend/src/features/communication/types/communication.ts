export const NOTIFICATION_TYPES = [
  "TicketCreated",
  "TicketUpdated",
  "TicketStatusChanged",
  "TicketCancelled",
  "TicketAssigned",
  "TicketReassigned",
  "TicketEscalated",
  "CommentAdded",
  "ReplyAdded",
  "AgentMentioned",
  "InternalNoteAdded",
] as const;

export type NotificationType = (typeof NOTIFICATION_TYPES)[number];
export type NotificationFilter = "all" | "unread" | "read";

export interface CommunicationUser {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
}

export interface TicketComment {
  id: string;
  ticketId: string;
  parentCommentId: string | null;
  content: string;
  author: CommunicationUser;
  mentions: CommunicationUser[];
  createdAtUtc: string;
}

export interface TicketCommentsResponse {
  items: TicketComment[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface AddTicketCommentInput {
  content: string;
  parentCommentId: string | null;
  mentionedAgentIds: string[];
}

export type MentionableAgent = CommunicationUser;

export interface NotificationActor {
  id: string;
  firstName: string;
  lastName: string;
}

export interface Notification {
  id: string;
  actor: NotificationActor;
  ticketId: string | null;
  type: NotificationType;
  title: string;
  message: string;
  isRead: boolean;
  createdAtUtc: string;
  readAtUtc: string | null;
}

export interface NotificationsResponse {
  items: Notification[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  unreadCount: number;
}

export interface NotificationFilters {
  isRead?: boolean;
  pageNumber: number;
  pageSize: number;
}

export interface UnreadNotificationCountResponse {
  unreadCount: number;
}

export interface MarkAllNotificationsReadResponse {
  markedCount: number;
}
