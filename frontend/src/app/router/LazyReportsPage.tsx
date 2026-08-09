import { lazy, Suspense } from "react";

const ReportsPage = lazy(() => import("@/features/reporting/pages/ReportsPage"));

export default function LazyReportsPage() {
  return (
    <Suspense fallback={<div className="flex min-h-screen items-center justify-center bg-muted/35 text-sm text-muted-foreground">Loading reports…</div>}>
      <ReportsPage />
    </Suspense>
  );
}
