import { apiClient } from "@/shared/api/apiClient";
import type {
  CreateUserRequest,
  GetUsersRequest,
  GetUsersResponse,
  UpdateUserRequest,
} from "../types/user";

export async function getUsers(
  request: GetUsersRequest,
): Promise<GetUsersResponse> {
  const response = await apiClient.get<GetUsersResponse>("/users", {
    params: request,
  });

  return response.data;
}

export async function createUser(request: CreateUserRequest): Promise<void> {
  await apiClient.post("/users", request);
}

export async function updateUser(
  id: string,
  request: UpdateUserRequest,
): Promise<void> {
  await apiClient.put(`/users/${id}`, request);
}

export async function deactivateUser(id: string): Promise<void> {
  await apiClient.delete(`/users/${id}`);
}
