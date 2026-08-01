export const USER_ROLES = [
  "Admin",
  "ITSupportSpecialist",
  "Manager",
  "Employee",
] as const;

export type UserRole = (typeof USER_ROLES)[number];

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  isActive: boolean;
  roles: string[];
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
  roles: string[];
}

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  isActive: boolean;
  roles: string[];
}
