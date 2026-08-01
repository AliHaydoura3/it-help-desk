import { Navigate } from "react-router-dom";

import { useAuth } from "@/features/auth/hooks/useAuth";
import { getDefaultRoute } from "@/features/auth/utils/getDefaultRoute";

export default function RoleIndexRoute() {
  const { user } = useAuth();

  return <Navigate to={getDefaultRoute(user)} replace />;
}
