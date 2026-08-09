import type { AuthUser } from "../types/auth";

export function getDefaultRoute(user: AuthUser | null): string {
  if (user?.role === "Admin") return "/admin";
  if (user?.role === "Manager") return "/reports";
  return "/tickets";
}
