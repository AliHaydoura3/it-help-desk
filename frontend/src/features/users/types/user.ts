import type { UserRole } from "@/features/auth/authorization/roles";

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  isActive: boolean;
  role: UserRole;
}

export interface GetUsersRequest {
  pageNumber: number;
  pageSize: number;
  search?: string;
  isActive?: boolean;
}

export interface GetUsersResponse {
  items: User[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  activeCount: number;
  inactiveCount: number;
  administratorCount: number;
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: UserRole;
}

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  isActive: boolean;
  role: UserRole;
}
