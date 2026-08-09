import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { NotificationRealtimeBridge } from "@/features/communication/realtime/NotificationRealtimeBridge";

export default function ProtectedRoute() {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return (
    <>
      <NotificationRealtimeBridge />
      <Outlet />
    </>
  );
}
