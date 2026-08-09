import type { UserRole } from "@/features/auth/authorization/roles";

export interface Profile {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: UserRole;
}

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  email: string;
}
