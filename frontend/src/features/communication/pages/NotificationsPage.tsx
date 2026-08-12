import { Bell, CheckCheck, Inbox, LoaderCircle } from "lucide-react";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { getApiErrorMessage } from "@/features/users/utils/getApiErrorMessage";
import { cn } from "@/lib/utils";
import { useNotificationMutations, useNotifications } from "../hooks/useNotifications";
import type { Notification, NotificationFilter } from "../types/communication";
import { formatRelativeTime, getFullName, getInitials, getNotificationIcon } from "../utils/communicationPresentation";
import { CommunicationPagination } from "../components/CommunicationPagination";

const PAGE_SIZE = 12;

export default function NotificationsPage() {
  const navigate = useNavigate();
  const [filter, setFilter] = useState<NotificationFilter>("all");
  const [page, setPage] = useState(1);
  const query = useNotifications({
    isRead: filter === "all" ? undefined : filter === "read",
    pageNumber: page,
    pageSize: PAGE_SIZE,
  });
  const mutations = useNotificationMutations();

  useEffect(() => {
    const totalPages = query.data?.totalPages;
    if (totalPages !== undefined && page > Math.max(1, totalPages)) setPage(Math.max(1, totalPages));
  }, [page, query.data?.totalPages]);

  async function markAllRead() {
    try {
      const result = await mutations.markAllRead.mutateAsync();
      toast.success(result.markedCount === 0 ? "You are already caught up." : `${result.markedCount} notification${result.markedCount === 1 ? "" : "s"} marked as read.`);
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Unable to update notifications."));
    }
  }

  async function openNotification(notification: Notification) {
    try {
      if (!notification.isRead) await mutations.markRead.mutateAsync(notification.id);
      if (notification.ticketId) navigate(`/tickets?conversation=${notification.ticketId}`);
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Unable to open this notification."));
    }
  }

  return (
    <main className="px-4 py-8 sm:px-6 lg:px-8">
      <div className="mx-auto max-w-5xl">
        <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
          <div className="flex items-center gap-4">
            <div className="flex size-11 items-center justify-center rounded-xl bg-primary text-primary-foreground"><Bell className="size-5" /></div>
            <div>
              <h1 className="text-3xl font-semibold tracking-tight">Notifications</h1>
              <p className="mt-1 text-sm text-muted-foreground">Ticket updates, assignments, replies, and mentions in one place.</p>
            </div>
          </div>
          <Button disabled={mutations.markAllRead.isPending || (query.data?.unreadCount ?? 0) === 0} onClick={markAllRead} variant="outline">
            {mutations.markAllRead.isPending ? <LoaderCircle className="animate-spin" /> : <CheckCheck />} Mark all read
          </Button>
        </div>

        <Card className="mt-8 gap-0 py-0 shadow-sm">
          <div className="flex flex-col gap-3 border-b p-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex gap-1 rounded-xl bg-muted p-1">
              {(["all", "unread", "read"] as const).map((value) => (
                <button
                  aria-pressed={filter === value}
                  className={cn("rounded-lg px-3 py-1.5 text-sm font-medium capitalize transition-colors", filter === value ? "bg-card text-foreground shadow-sm" : "text-muted-foreground hover:text-foreground")}
                  key={value}
                  onClick={() => { setFilter(value); setPage(1); }}
                >
                  {value}
                </button>
              ))}
            </div>
            <p className="text-xs text-muted-foreground">{query.data?.unreadCount ?? 0} unread</p>
          </div>

          {query.isLoading ? (
            <div className="flex min-h-96 items-center justify-center"><LoaderCircle className="animate-spin text-muted-foreground" /></div>
          ) : query.isError ? (
            <div className="flex min-h-96 flex-col items-center justify-center px-6 text-center">
              <p className="font-medium">Could not load notifications</p>
              <p className="mt-1 text-sm text-muted-foreground">Check your connection and try again.</p>
              <Button className="mt-4" onClick={() => query.refetch()} variant="outline">Try again</Button>
            </div>
          ) : query.data?.items.length === 0 ? (
            <div className="flex min-h-96 flex-col items-center justify-center px-6 text-center">
              <div className="flex size-12 items-center justify-center rounded-full bg-muted text-muted-foreground"><Inbox className="size-5" /></div>
              <p className="mt-4 font-medium">No {filter === "all" ? "" : `${filter} `}notifications</p>
              <p className="mt-1 text-sm text-muted-foreground">Ticket activity addressed to you will appear here.</p>
            </div>
          ) : (
            <div className="divide-y">
              {query.data?.items.map((notification) => (
                <NotificationRow
                  disabled={mutations.markRead.isPending}
                  key={notification.id}
                  notification={notification}
                  onOpen={openNotification}
                />
              ))}
            </div>
          )}

          {query.data && query.data.totalCount > 0 && (
            <CommunicationPagination
              disabled={query.isFetching}
              noun="notifications"
              onPageChange={setPage}
              pageNumber={query.data.pageNumber}
              pageSize={query.data.pageSize}
              totalCount={query.data.totalCount}
              totalPages={query.data.totalPages}
            />
          )}
        </Card>
      </div>
    </main>
  );
}

function NotificationRow({ notification, disabled, onOpen }: { notification: Notification; disabled: boolean; onOpen: (notification: Notification) => void }) {
  const Icon = getNotificationIcon(notification.type);
  const actorName = getFullName(notification.actor);

  return (
    <article className={cn("relative flex gap-4 px-4 py-4 transition-colors sm:px-5", !notification.isRead && "bg-primary/[0.035]")}>
      {!notification.isRead && <span aria-label="Unread" className="absolute left-1.5 top-1/2 size-2 -translate-y-1/2 rounded-full bg-primary" />}
      <div className="relative hidden shrink-0 sm:block">
        <div className="flex size-11 items-center justify-center rounded-full bg-muted text-xs font-semibold">{getInitials(notification.actor.firstName, notification.actor.lastName)}</div>
        <div className="absolute -bottom-1 -right-1 flex size-6 items-center justify-center rounded-full border-2 border-card bg-primary text-primary-foreground"><Icon className="size-3" /></div>
      </div>
      <div className="min-w-0 flex-1">
        <div className="flex flex-col justify-between gap-1 sm:flex-row sm:items-start">
          <div>
            <p className={cn("text-sm", !notification.isRead && "font-semibold")}>{notification.title}</p>
            <p className="mt-1 text-sm leading-5 text-muted-foreground">{notification.message}</p>
          </div>
          <time className="shrink-0 text-xs text-muted-foreground" dateTime={notification.createdAtUtc} title={new Date(notification.createdAtUtc).toLocaleString()}>{formatRelativeTime(notification.createdAtUtc)}</time>
        </div>
        <div className="mt-2 flex flex-wrap items-center justify-between gap-2">
          <p className="text-xs text-muted-foreground">Activity by {actorName}</p>
          <Button disabled={disabled} onClick={() => onOpen(notification)} size="sm" variant={notification.ticketId ? "outline" : "ghost"}>
            {notification.ticketId ? "View ticket conversation" : notification.isRead ? "Read" : "Mark as read"}
          </Button>
        </div>
      </div>
    </article>
  );
}
