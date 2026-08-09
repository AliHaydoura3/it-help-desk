import { Bell } from "lucide-react";
import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { useUnreadNotificationCount } from "../hooks/useNotifications";

export function NotificationBell({ className }: { className?: string }) {
  const countQuery = useUnreadNotificationCount();
  const count = countQuery.data?.unreadCount ?? 0;
  const label = count === 0 ? "Notifications" : `${count} unread notification${count === 1 ? "" : "s"}`;

  return (
    <Button
      aria-label={label}
      className={cn("relative", className)}
      render={<Link to="/notifications" />}
      size="icon"
      variant="ghost"
    >
      <Bell />
      {count > 0 && (
        <span className="absolute -right-0.5 -top-0.5 flex min-w-4.5 items-center justify-center rounded-full bg-destructive px-1 text-[10px] font-semibold leading-4 text-white ring-2 ring-card">
          {count > 99 ? "99+" : count}
        </span>
      )}
    </Button>
  );
}
