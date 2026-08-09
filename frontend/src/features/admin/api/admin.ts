import { apiClient } from "@/shared/api/apiClient";
import type {
  AdminDashboard,
  AdminRole,
  SystemSettings,
  TicketCategorySetting,
  UpdateSystemSettingsRequest,
  UpdateTicketCategoryRequest,
} from "../types/admin";

export async function getAdminDashboard(): Promise<AdminDashboard> {
  return (await apiClient.get<AdminDashboard>("/admin/dashboard")).data;
}

export async function getAdminRoles(): Promise<AdminRole[]> {
  return (await apiClient.get<AdminRole[]>("/admin/roles")).data;
}

export async function getAdminTicketCategories(): Promise<TicketCategorySetting[]> {
  return (await apiClient.get<TicketCategorySetting[]>("/admin/ticket-categories")).data;
}

export async function updateAdminTicketCategory(
  request: UpdateTicketCategoryRequest,
): Promise<TicketCategorySetting> {
  return (await apiClient.put<TicketCategorySetting>(
    `/admin/ticket-categories/${request.category}`,
    request,
  )).data;
}

export async function getSystemSettings(): Promise<SystemSettings> {
  return (await apiClient.get<SystemSettings>("/admin/settings")).data;
}

export async function updateSystemSettings(
  request: UpdateSystemSettingsRequest,
): Promise<SystemSettings> {
  return (await apiClient.put<SystemSettings>("/admin/settings", request)).data;
}
