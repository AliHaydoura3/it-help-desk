import { useNavigate } from "react-router-dom";
import { useAuth } from "@/features/auth/hooks/useAuth";

export default function DashboardPage() {
  const auth = useAuth();
  const navigate = useNavigate();

  function logout() {
    auth.logout();

    navigate("/login");
  }

  return (
    <>
      <h1>Dashboard</h1>

      <button onClick={logout}>Logout</button>
    </>
  );
}
