import { createBrowserRouter } from "react-router-dom";

import ProtectedRoute from "./ProtectedRoute";
import AdminRoute from "./AdminRoute";
import GuestRoute from "./GuestRoute";
import RoleIndexRoute from "./RoleIndexRoute";

import LoginPage from "@/features/auth/pages/LoginPage";
import DashboardPage from "@/features/dashboard/pages/DashboardPage";
import AccessDeniedPage from "@/features/auth/pages/AccessDeniedPage";
import ForgotPasswordPage from "@/features/auth/pages/ForgotPasswordPage";
import ResetPasswordPage from "@/features/auth/pages/ResetPasswordPage";
import ProfilePage from "@/features/profile/pages/ProfilePage";
import ActivityLogsPage from "@/features/activity/pages/ActivityLogsPage";
import TicketsPage from "@/features/tickets/pages/TicketsPage";

export const router = createBrowserRouter([
  {
    element: <GuestRoute />,
    children: [
      {
        path: "/login",
        element: <LoginPage />,
      },
      {
        path: "/forgot-password",
        element: <ForgotPasswordPage />,
      },
      {
        path: "/reset-password",
        element: <ResetPasswordPage />,
      },
    ],
  },
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: "/",
        element: <RoleIndexRoute />,
      },
      {
        path: "/access-denied",
        element: <AccessDeniedPage />,
      },
      {
        path: "/profile",
        element: <ProfilePage />,
      },
      {
        path: "/tickets",
        element: <TicketsPage />,
      },
      {
        element: <AdminRoute />,
        children: [
          {
            path: "/users",
            element: <DashboardPage />,
          },
          {
            path: "/activity-logs",
            element: <ActivityLogsPage />,
          },
        ],
      },
    ],
  },
]);
