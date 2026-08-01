export const TICKET_CATEGORIES = ["Hardware", "Software", "Network", "Email", "AccessRequest", "Other"] as const;
export const TICKET_PRIORITIES = ["Low", "Medium", "High", "Critical"] as const;
export const TICKET_STATUSES = ["Open", "InProgress", "Pending", "Resolved", "Closed"] as const;
export const ESCALATION_LEVELS = ["Level1", "Level2", "Level3"] as const;

export type TicketCategory = (typeof TICKET_CATEGORIES)[number];
export type TicketPriority = (typeof TICKET_PRIORITIES)[number];
export type TicketStatus = (typeof TICKET_STATUSES)[number];
export type TicketEscalationLevel = "None" | (typeof ESCALATION_LEVELS)[number];
export type TicketAssignmentType = "Manual" | "Automatic" | "Reassignment";

export interface Ticket {
  id: string;
  referenceNumber: string;
  title: string;
  description: string;
  category: TicketCategory;
  priority: TicketPriority;
  status: TicketStatus;
  escalationLevel: TicketEscalationLevel;
  isCancelled: boolean;
  createdByUserId: string;
  assignedToUserId: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface TicketInput {
  title: string;
  description: string;
  category: TicketCategory;
  priority: TicketPriority;
}

export interface TicketFilters {
  search?: string;
  category?: TicketCategory;
  priority?: TicketPriority;
  status?: TicketStatus;
  pageNumber: number;
  pageSize: number;
}

export interface TicketListResponse {
  items: Ticket[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface TicketHistoryItem {
  action: string;
  previousValue: string | null;
  newValue: string | null;
  actorUserId: string;
  occurredAtUtc: string;
}

export interface TicketSummary {
  total: number;
  open: number;
  inProgress: number;
  pending: number;
  resolved: number;
  closed: number;
  critical: number;
}

export interface AssignableAgent {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  activeTicketCount: number;
}

export interface AssignmentHistoryItem {
  previousAgentId: string | null;
  assignedAgentId: string;
  actorUserId: string;
  assignmentType: TicketAssignmentType;
  occurredAtUtc: string;
}

export interface InternalNote {
  id: string;
  authorUserId: string;
  content: string;
  createdAtUtc: string;
}
