import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getAdminDashboard,
  getAdminRoles,
  getAdminTicketCategories,
  getSystemSettings,
  updateAdminTicketCategory,
  updateSystemSettings,
} from "../api/admin";
import type { UpdateSystemSettingsRequest, UpdateTicketCategoryRequest } from "../types/admin";

export const adminKeys = {
  root: ["admin"] as const,
  dashboard: ["admin", "dashboard"] as const,
  roles: ["admin", "roles"] as const,
  categories: ["admin", "ticket-categories"] as const,
  settings: ["admin", "settings"] as const,
};

export function useAdminDashboard() {
  return useQuery({ queryKey: adminKeys.dashboard, queryFn: getAdminDashboard });
}

export function useAdminRoles() {
  return useQuery({ queryKey: adminKeys.roles, queryFn: getAdminRoles });
}

export function useAdminTicketCategories() {
  return useQuery({ queryKey: adminKeys.categories, queryFn: getAdminTicketCategories });
}

export function useUpdateAdminTicketCategory() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: UpdateTicketCategoryRequest) => updateAdminTicketCategory(request),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: adminKeys.categories }),
        queryClient.invalidateQueries({ queryKey: ["ticket-categories"] }),
      ]);
    },
  });
}

export function useSystemSettings() {
  return useQuery({ queryKey: adminKeys.settings, queryFn: getSystemSettings });
}

export function useUpdateSystemSettings() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: UpdateSystemSettingsRequest) => updateSystemSettings(request),
    onSuccess: (settings) => queryClient.setQueryData(adminKeys.settings, settings),
  });
}
