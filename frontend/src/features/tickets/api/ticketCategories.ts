import { apiClient } from "@/shared/api/apiClient";
import type { TicketCategorySetting } from "@/features/admin/types/admin";

export async function getActiveTicketCategories(): Promise<TicketCategorySetting[]> {
  return (await apiClient.get<TicketCategorySetting[]>("/ticket-categories")).data;
}
