import type { UserRole } from "../authorization/roles";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
}

export interface AuthUser {
  id: string;
  email: string;
  role: UserRole;
}
