import { apiClient } from "@/shared/api/apiClient";
import type { Profile, UpdateProfileRequest } from "../types/profile";

export async function getProfile(): Promise<Profile> {
  return (await apiClient.get<Profile>("/profile")).data;
}

export async function updateProfile(request: UpdateProfileRequest): Promise<Profile> {
  return (await apiClient.put<Profile>("/profile", request)).data;
}

export async function changePassword(request: {
  currentPassword: string;
  newPassword: string;
}): Promise<void> {
  await apiClient.post("/profile/change-password", request);
}
