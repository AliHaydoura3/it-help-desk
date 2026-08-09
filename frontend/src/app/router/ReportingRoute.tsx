import { Navigate, Outlet } from "react-router-dom";

import { hasPermission } from "@/features/auth/authorization/roles";
import { useAuth } from "@/features/auth/hooks/useAuth";

export default function ReportingRoute() {
  const { user } = useAuth();

  if (!hasPermission(user, "view-ticket-reports")) {
    return <Navigate to="/access-denied" replace />;
  }

  return <Outlet />;
}
