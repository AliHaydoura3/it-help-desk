import { lazy, Suspense } from "react";

const AdminPage = lazy(() => import("@/features/admin/pages/AdminPage"));

export default function LazyAdminPage() {
  return <Suspense fallback={<div className="flex min-h-screen items-center justify-center bg-muted/35 text-sm text-muted-foreground">Loading administration…</div>}>
    <AdminPage />
  </Suspense>;
}
