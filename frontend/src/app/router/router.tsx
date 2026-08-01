import { createBrowserRouter } from "react-router-dom";

import ProtectedRoute from "./ProtectedRoute";
import AdminRoute from "./AdminRoute";
import GuestRoute from "./GuestRoute";

import LoginPage from "@/features/auth/pages/LoginPage";
import DashboardPage from "@/features/dashboard/pages/DashboardPage";
import AccessDeniedPage from "@/features/auth/pages/AccessDeniedPage";

export const router = createBrowserRouter([
  {
    element: <GuestRoute />,
    children: [
      {
        path: "/login",
        element: <LoginPage />,
      },
    ],
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: "/access-denied",
        element: <AccessDeniedPage />,
      },
      {
        element: <AdminRoute />,
        children: [
          {
            path: "/",
            element: <DashboardPage />,
          },
        ],
      },
    ],
  },
]);
