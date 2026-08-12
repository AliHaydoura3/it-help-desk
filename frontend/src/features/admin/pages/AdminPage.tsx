import { Navigate, useParams } from "react-router-dom";
import { AdminOverview } from "../components/AdminOverview";
import { CategoryManagement } from "../components/CategoryManagement";
import { RoleManagement } from "../components/RoleManagement";
import { SystemSettingsPanel } from "../components/SystemSettingsPanel";

export default function AdminPage() {
  const { section } = useParams();
  const content = section === undefined ? <AdminOverview />
    : section === "roles" ? <RoleManagement />
    : section === "categories" ? <CategoryManagement />
    : section === "settings" ? <SystemSettingsPanel />
    : null;
  if (!content) return <Navigate replace to="/admin" />;
  return <main className="mx-auto max-w-7xl px-4 py-7 sm:px-6 lg:px-8 lg:py-9">{content}</main>;
}
