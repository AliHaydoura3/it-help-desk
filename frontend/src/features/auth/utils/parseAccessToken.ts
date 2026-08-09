import type { AuthUser } from "../types/auth";
import { isUserRole, USER_ROLES } from "../authorization/roles";

const ROLE_CLAIM =
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

interface JwtPayload {
  sub?: string;
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"?: string;
  email?: string;
  role?: string | string[];
  [ROLE_CLAIM]?: string | string[];
}

export function parseAccessToken(token: string | null): AuthUser | null {
  if (!token) return null;

  try {
    const encodedPayload = token.split(".")[1];
    if (!encodedPayload) return null;

    const base64 = encodedPayload
      .replaceAll("-", "+")
      .replaceAll("_", "/")
      .padEnd(Math.ceil(encodedPayload.length / 4) * 4, "=");
    const payload = JSON.parse(atob(base64)) as JwtPayload;
    const roleClaim = payload[ROLE_CLAIM] ?? payload.role ?? [];
    const claimedRoles = Array.isArray(roleClaim) ? roleClaim : [roleClaim];
    const role = USER_ROLES.find((candidate) => claimedRoles.includes(candidate));

    if (!role || !isUserRole(role)) return null;

    return {
      id:
        payload.sub ??
        payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ??
        "",
      email: payload.email ?? "Administrator",
      role,
    };
  } catch {
    return null;
  }
}
