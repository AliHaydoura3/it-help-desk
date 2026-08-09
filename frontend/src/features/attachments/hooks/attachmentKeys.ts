export const attachmentKeys = {
  all: ["attachments"] as const,
  policy: ["attachments", "policy"] as const,
  ticket: (ticketId: string) => ["attachments", "tickets", ticketId] as const,
  page: (ticketId: string, pageNumber: number, pageSize: number) =>
    ["attachments", "tickets", ticketId, { pageNumber, pageSize }] as const,
};
