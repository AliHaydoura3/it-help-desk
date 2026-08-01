import { z } from "zod";
import { TICKET_CATEGORIES, TICKET_PRIORITIES } from "../types/ticket";

export const ticketSchema = z.object({
  title: z.string().trim().min(1, "Title is required").max(200),
  description: z.string().trim().min(1, "Description is required").max(4000),
  category: z.enum(TICKET_CATEGORIES),
  priority: z.enum(TICKET_PRIORITIES),
});

export type TicketFormData = z.infer<typeof ticketSchema>;
