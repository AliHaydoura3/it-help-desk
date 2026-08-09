import { useQuery } from "@tanstack/react-query";
import { getActiveTicketCategories } from "../api/ticketCategories";

export function useTicketCategories() {
  return useQuery({
    queryKey: ["ticket-categories", "active"],
    queryFn: getActiveTicketCategories,
    staleTime: 60_000,
  });
}
