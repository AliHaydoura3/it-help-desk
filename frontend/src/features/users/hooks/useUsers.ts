import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createUser,
  deactivateUser,
  getUsers,
  updateUser,
} from "../api/users";
import type {
  CreateUserRequest,
  GetUsersRequest,
  UpdateUserRequest,
} from "../types/user";

const usersQueryKey = ["users"] as const;

export function useUsers(request: GetUsersRequest) {
  return useQuery({
    queryKey: [...usersQueryKey, request],
    queryFn: () => getUsers(request),
    placeholderData: (previousData) => previousData,
  });
}

export function useCreateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request: CreateUserRequest) => createUser(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: usersQueryKey }),
  });
}

export function useUpdateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateUserRequest }) =>
      updateUser(id, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: usersQueryKey }),
  });
}

export function useDeactivateUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: deactivateUser,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: usersQueryKey }),
  });
}
