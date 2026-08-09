import { Navigate, useParams } from "react-router-dom";
import { AdminShell } from "../components/AdminShell";
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
  return <AdminShell>{content}</AdminShell>;
}
