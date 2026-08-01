import { apiClient } from "@/shared/api/apiClient";
import { getRefreshToken } from "../utils/tokenStorage";

export async function logoutSession(): Promise<void> {
  const refreshToken = getRefreshToken();

  if (refreshToken) {
    await apiClient.post("/auth/logout", { refreshToken });
  }
}
