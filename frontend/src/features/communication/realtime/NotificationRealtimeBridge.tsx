import { HubConnectionBuilder, HubConnectionState, LogLevel } from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { toast } from "sonner";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { communicationKeys } from "../hooks/communicationKeys";
import type { Notification } from "../types/communication";

type RealtimeNotification = Omit<Notification, "readAtUtc"> & {
  recipientUserId: string;
};

const NOTIFICATION_EVENT = "notificationReceived";

export function NotificationRealtimeBridge() {
  const auth = useAuth();
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!auth.accessToken) return;

    const connection = new HubConnectionBuilder()
      .withUrl(getNotificationHubUrl(), {
        accessTokenFactory: () => auth.accessToken ?? "",
      })
      .withAutomaticReconnect([0, 2_000, 5_000, 10_000, 30_000])
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on(NOTIFICATION_EVENT, (notification: RealtimeNotification) => {
      void queryClient.invalidateQueries({ queryKey: communicationKeys.notifications });
      if (notification.ticketId) {
        void queryClient.invalidateQueries({ queryKey: communicationKeys.comments(notification.ticketId) });
        void queryClient.invalidateQueries({ queryKey: ["tickets"] });
      }
      toast.info(notification.title, { description: notification.message });
    });

    void connection.start().catch(() => {
      // The notification queries poll as a fallback while real-time delivery reconnects.
    });

    return () => {
      connection.off(NOTIFICATION_EVENT);
      if (connection.state !== HubConnectionState.Disconnected) void connection.stop();
    };
  }, [auth.accessToken, queryClient]);

  return null;
}

function getNotificationHubUrl(): string {
  const configured = import.meta.env.VITE_NOTIFICATION_HUB_URL as string | undefined;
  if (configured) return configured;

  const apiUrl = new URL(import.meta.env.VITE_API_URL, window.location.origin);
  const apiRoot = apiUrl.pathname.replace(/\/api\/?$/, "").replace(/\/$/, "");
  return `${apiUrl.origin}${apiRoot}/hubs/notifications`;
}
