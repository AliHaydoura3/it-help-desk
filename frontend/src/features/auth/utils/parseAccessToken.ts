import type { AuthUser } from "../types/auth";

const ROLE_CLAIM =
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

interface JwtPayload {
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

    return {
      email: payload.email ?? "Administrator",
      roles: Array.isArray(roleClaim) ? roleClaim : [roleClaim],
    };
  } catch {
    return null;
  }
}
