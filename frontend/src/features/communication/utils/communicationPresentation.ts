import {
  ArrowUpRight,
  AtSign,
  BellRing,
  CircleCheck,
  GitBranch,
  MessageCircle,
  Pencil,
  TicketCheck,
  Trash2,
  type LucideIcon,
} from "lucide-react";
import type { NotificationType } from "../types/communication";

export function getInitials(firstName: string, lastName: string): string {
  return `${firstName[0] ?? ""}${lastName[0] ?? ""}`.toUpperCase() || "?";
}

export function getFullName(user: { firstName: string; lastName: string }): string {
  return `${user.firstName} ${user.lastName}`.trim();
}

export function formatRelativeTime(value: string): string {
  const timestamp = new Date(value).getTime();
  const seconds = Math.round((timestamp - Date.now()) / 1_000);
  const absolute = Math.abs(seconds);
  const formatter = new Intl.RelativeTimeFormat(undefined, { numeric: "auto" });

  if (absolute < 60) return formatter.format(seconds, "second");
  const minutes = Math.round(seconds / 60);
  if (Math.abs(minutes) < 60) return formatter.format(minutes, "minute");
  const hours = Math.round(minutes / 60);
  if (Math.abs(hours) < 24) return formatter.format(hours, "hour");
  const days = Math.round(hours / 24);
  if (Math.abs(days) < 7) return formatter.format(days, "day");
  return new Date(value).toLocaleDateString();
}

export function getNotificationIcon(type: NotificationType): LucideIcon {
  const icons: Record<NotificationType, LucideIcon> = {
    TicketCreated: TicketCheck,
    TicketUpdated: Pencil,
    TicketStatusChanged: CircleCheck,
    TicketCancelled: Trash2,
    TicketAssigned: GitBranch,
    TicketReassigned: GitBranch,
    TicketEscalated: ArrowUpRight,
    CommentAdded: MessageCircle,
    ReplyAdded: MessageCircle,
    AgentMentioned: AtSign,
    InternalNoteAdded: BellRing,
  };

  return icons[type];
}
