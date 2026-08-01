import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { getDefaultRoute } from "@/features/auth/utils/getDefaultRoute";

export default function GuestRoute() {
    const { isAuthenticated, user } = useAuth();

    if (isAuthenticated) {
        return <Navigate to={getDefaultRoute(user)} replace />;
    }

    return <Outlet />;
}
