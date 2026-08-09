export const USER_ROLES = [
  "Admin",
  "ITSupportSpecialist",
  "Manager",
  "Employee",
] as const;

export type UserRole = (typeof USER_ROLES)[number];

export const ROLE_LABELS: Record<UserRole, string> = {
  Admin: "Administrator",
  ITSupportSpecialist: "IT Support Agent",
  Manager: "Manager",
  Employee: "Employee",
};

export const ROLE_DESCRIPTIONS: Record<UserRole, string> = {
  Admin: "Full system access with override authority across users, tickets, workflow, and audit data.",
  ITSupportSpecialist: "Employee access plus queue triage, assignment, assigned-ticket status changes, and internal notes.",
  Manager: "Employee access plus read-only ticket monitoring, assignment history, and operational reports.",
  Employee: "Create and track personal tickets, update or cancel open requests, and participate in their conversations.",
};

export type Permission =
  | "manage-users"
  | "monitor-tickets"
  | "edit-all-tickets"
  | "cancel-all-tickets"
  | "change-assigned-ticket-status"
  | "change-any-ticket-status"
  | "comment-on-all-tickets"
  | "view-ticket-reports"
  | "manage-ticket-workflow"
  | "view-assignment-history"
  | "use-internal-notes";

const ROLE_PERMISSIONS: Record<UserRole, readonly Permission[]> = {
  Admin: [
    "manage-users",
    "monitor-tickets",
    "edit-all-tickets",
    "cancel-all-tickets",
    "change-assigned-ticket-status",
    "change-any-ticket-status",
    "comment-on-all-tickets",
    "view-ticket-reports",
    "manage-ticket-workflow",
    "view-assignment-history",
    "use-internal-notes",
  ],
  ITSupportSpecialist: [
    "monitor-tickets",
    "edit-all-tickets",
    "cancel-all-tickets",
    "change-assigned-ticket-status",
    "comment-on-all-tickets",
    "manage-ticket-workflow",
    "view-assignment-history",
    "use-internal-notes",
  ],
  Manager: [
    "monitor-tickets",
    "view-ticket-reports",
    "view-assignment-history",
  ],
  Employee: [],
};

export function isUserRole(value: string): value is UserRole {
  return USER_ROLES.some((role) => role === value);
}

export function hasPermission(user: { role: UserRole } | null, permission: Permission): boolean {
  return user ? ROLE_PERMISSIONS[user.role].includes(permission) : false;
}
