import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  getNotifications,
  getUnreadNotificationCount,
  markAllNotificationsRead,
  markNotificationRead,
} from "../api/communication";
import type { NotificationFilters } from "../types/communication";
import { communicationKeys } from "./communicationKeys";

export function useNotifications(filters: NotificationFilters) {
  return useQuery({
    queryKey: communicationKeys.notificationsPage(filters),
    queryFn: () => getNotifications(filters),
    placeholderData: (previous) => previous,
    refetchInterval: 30_000,
  });
}

export function useUnreadNotificationCount() {
  return useQuery({
    queryKey: communicationKeys.unreadCount,
    queryFn: getUnreadNotificationCount,
    refetchInterval: 30_000,
  });
}

export function useNotificationMutations() {
  const queryClient = useQueryClient();
  const refresh = () => queryClient.invalidateQueries({ queryKey: communicationKeys.notifications });

  return {
    markRead: useMutation({ mutationFn: markNotificationRead, onSuccess: refresh }),
    markAllRead: useMutation({ mutationFn: markAllNotificationsRead, onSuccess: refresh }),
  };
}
