import type { AuthUser } from "../types/auth";

export function getDefaultRoute(user: AuthUser | null): string {
  return user?.roles.includes("Admin") ? "/users" : "/tickets";
}
