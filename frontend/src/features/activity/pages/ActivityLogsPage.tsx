import { useState } from "react";
import { ChevronLeft, ChevronRight, CircleCheck, CircleX, History } from "lucide-react";
import { useQuery } from "@tanstack/react-query";

import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { getActivityLogs } from "../api/activityLogs";

export default function ActivityLogsPage() {
  const [page, setPage] = useState(1);
  const query = useQuery({
    queryKey: ["activity-logs", page],
    queryFn: () => getActivityLogs(page),
    placeholderData: (previous) => previous,
  });

  return (
    <main className="px-4 py-8 sm:px-6 lg:px-8">
      <div className="mx-auto max-w-6xl">
        <div className="flex items-center gap-4">
          <div className="flex size-11 items-center justify-center rounded-xl bg-primary text-primary-foreground"><History className="size-5" /></div>
          <div>
            <h1 className="text-3xl font-semibold tracking-tight">Activity logs</h1>
            <p className="mt-1 text-sm text-muted-foreground">Review authentication and API activity across the system.</p>
          </div>
        </div>

        <Card className="mt-8 gap-0 overflow-hidden py-0 shadow-sm">
          {query.isLoading ? (
            <div className="py-20 text-center text-sm text-muted-foreground">Loading activity…</div>
          ) : query.isError ? (
            <div className="py-20 text-center text-sm text-destructive">Unable to load activity logs.</div>
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full min-w-3xl text-left text-sm">
                  <thead className="border-b bg-muted/40 text-xs uppercase tracking-wide text-muted-foreground">
                    <tr><th className="px-5 py-3">User</th><th className="px-5 py-3">Action</th><th className="px-5 py-3">Resource</th><th className="px-5 py-3">Status</th><th className="px-5 py-3">Time</th></tr>
                  </thead>
                  <tbody className="divide-y">
                    {query.data?.items.map((log) => (
                      <tr key={log.id} className="hover:bg-muted/25">
                        <td className="px-5 py-4"><p className="font-medium">{log.userEmail ?? "Anonymous"}</p><p className="text-xs text-muted-foreground">{log.ipAddress ?? "Unknown IP"}</p></td>
                        <td className="px-5 py-4"><span className="rounded-md bg-secondary px-2 py-1 text-xs font-semibold">{log.action}</span></td>
                        <td className="px-5 py-4 font-mono text-xs text-muted-foreground">{log.resource}</td>
                        <td className="px-5 py-4">{log.succeeded ? <span className="inline-flex items-center gap-1 text-xs text-emerald-700"><CircleCheck className="size-4" /> Success</span> : <span className="inline-flex items-center gap-1 text-xs text-destructive"><CircleX className="size-4" /> Failed</span>}</td>
                        <td className="whitespace-nowrap px-5 py-4 text-xs text-muted-foreground">{new Date(log.occurredAtUtc).toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {(query.data?.totalCount ?? 0) === 0 && <div className="py-16 text-center text-sm text-muted-foreground">No activity has been recorded yet.</div>}
              {(query.data?.totalPages ?? 0) > 1 && (
                <div className="flex items-center justify-between border-t px-5 py-3">
                  <p className="text-xs text-muted-foreground">Page {query.data?.pageNumber} of {query.data?.totalPages}</p>
                  <div className="flex gap-1">
                    <Button aria-label="Previous page" disabled={page === 1} onClick={() => setPage((value) => value - 1)} size="icon-sm" variant="outline"><ChevronLeft /></Button>
                    <Button aria-label="Next page" disabled={page === query.data?.totalPages} onClick={() => setPage((value) => value + 1)} size="icon-sm" variant="outline"><ChevronRight /></Button>
                  </div>
                </div>
              )}
            </>
          )}
        </Card>
      </div>
    </main>
  );
}
