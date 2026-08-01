import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "@/features/auth/hooks/useAuth";

export default function AdminRoute() {
  const { user } = useAuth();

  if (!user?.roles.includes("Admin")) {
    return <Navigate to="/access-denied" replace />;
  }

  return <Outlet />;
}
