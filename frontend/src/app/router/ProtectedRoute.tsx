import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { NotificationRealtimeBridge } from "@/features/communication/realtime/NotificationRealtimeBridge";
import { WorkspaceShell } from "@/shared/layout/WorkspaceShell";

export default function ProtectedRoute() {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return (
    <WorkspaceShell>
      <NotificationRealtimeBridge />
      <Outlet />
    </WorkspaceShell>
  );
}
