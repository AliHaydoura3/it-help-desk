import { apiClient } from "@/shared/api/apiClient";

export async function forgotPassword(email: string): Promise<void> {
  await apiClient.post("/auth/forgot-password", { email });
}

export async function resetPassword(request: {
  email: string;
  token: string;
  newPassword: string;
}): Promise<void> {
  await apiClient.post("/auth/reset-password", request);
}
