import { useState, type ReactNode } from "react";
import {
  Activity, BarChart3, Bell, FolderCog, Gauge, LogOut, Menu,
  Settings, ShieldCheck, TicketCheck, UserRound, Users, X,
} from "lucide-react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { logoutSession } from "@/features/auth/api/logout";
import { ROLE_LABELS } from "@/features/auth/authorization/roles";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { NotificationBell } from "@/features/communication/components/NotificationBell";
import { cn } from "@/lib/utils";

type NavigationItem = {
  to: string;
  label: string;
  icon: typeof TicketCheck;
  exact?: boolean;
};

export function WorkspaceShell({ children }: { children: ReactNode }) {
  const auth = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [mobileOpen, setMobileOpen] = useState(false);
  const user = auth.user;
  const navigation = getNavigation(user?.role);
  const email = user?.email ?? "Account";
  const initials = email.split("@")[0].split(/[._-]/).map((part) => part[0] ?? "").join("").slice(0, 2).toUpperCase();

  async function logout() {
    try { await logoutSession(); } catch { /* local logout still completes */ }
    auth.logout();
    navigate("/login");
  }

  const navigationContent = <>
    <Brand />
    <nav className="flex-1 overflow-y-auto px-3 py-4" aria-label="Workspace navigation">
      <p className="mb-2 px-3 text-[11px] font-semibold uppercase tracking-[0.16em] text-muted-foreground">Workspace</p>
      {navigation.map(({ to, label, icon: Icon, exact }) => {
        const active = exact ? location.pathname === to : location.pathname === to || location.pathname.startsWith(`${to}/`);
        return <Link
          aria-current={active ? "page" : undefined}
          className={cn(
            "mb-1 flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm transition-colors",
            active ? "bg-primary font-medium text-primary-foreground shadow-sm" : "text-muted-foreground hover:bg-muted hover:text-foreground",
          )}
          key={to}
          onClick={() => setMobileOpen(false)}
          to={to}
        ><Icon className="size-4" />{label}</Link>;
      })}
    </nav>
    <div className="border-t p-3">
      <Link className={cn("flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm transition-colors", location.pathname === "/profile" ? "bg-primary font-medium text-primary-foreground" : "text-muted-foreground hover:bg-muted hover:text-foreground")} onClick={() => setMobileOpen(false)} to="/profile"><UserRound className="size-4" />My profile</Link>
      <button className="mt-1 flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground" onClick={logout}><LogOut className="size-4" />Sign out</button>
    </div>
  </>;

  return <div className="min-h-screen bg-muted/35">
    <aside className="fixed inset-y-0 left-0 z-40 hidden w-64 flex-col border-r bg-card lg:flex">{navigationContent}</aside>
    {mobileOpen && <div className="fixed inset-0 z-50 bg-foreground/35 backdrop-blur-sm lg:hidden" onClick={() => setMobileOpen(false)}>
      <aside className="relative flex h-full w-72 flex-col bg-card shadow-2xl" onClick={(event) => event.stopPropagation()}>
        {navigationContent}
        <Button aria-label="Close navigation" className="absolute right-3 top-3" onClick={() => setMobileOpen(false)} size="icon" variant="ghost"><X /></Button>
      </aside>
    </div>}
    <div className="lg:pl-64">
      <header className="sticky top-0 z-30 flex h-16 items-center border-b bg-background/90 px-4 backdrop-blur sm:px-6 lg:px-8">
        <Button aria-label="Open navigation" className="mr-3 lg:hidden" onClick={() => setMobileOpen(true)} size="icon" variant="ghost"><Menu /></Button>
        <div className="min-w-0 flex-1"><p className="truncate text-sm font-medium">{getSectionTitle(location.pathname)}</p><p className="hidden truncate text-xs text-muted-foreground sm:block">{user ? ROLE_LABELS[user.role] : "Workspace"}</p></div>
        <div className="flex items-center gap-3"><NotificationBell /><div className="hidden text-right md:block"><p className="max-w-48 truncate text-sm font-medium">{email}</p><p className="text-xs text-muted-foreground">{user ? ROLE_LABELS[user.role] : "Account"}</p></div><div className="flex size-9 items-center justify-center rounded-full bg-primary text-xs font-semibold text-primary-foreground">{initials || "U"}</div></div>
      </header>
      {children}
    </div>
  </div>;
}

function Brand() { return <div className="flex h-16 items-center gap-3 border-b px-5"><div className="flex size-9 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-sm"><ShieldCheck className="size-5" /></div><div><p className="text-sm font-semibold leading-tight">IT Help Desk</p><p className="text-xs text-muted-foreground">Support workspace</p></div></div>; }

function getNavigation(role: string | undefined): NavigationItem[] {
  const common: NavigationItem[] = [
    { to: "/tickets", label: "Tickets", icon: TicketCheck },
    { to: "/notifications", label: "Notifications", icon: Bell },
  ];
  if (role === "Manager") return [common[0], { to: "/reports", label: "Reports", icon: BarChart3 }, common[1]];
  if (role !== "Admin") return common;
  return [
    { to: "/admin", label: "System overview", icon: Gauge, exact: true },
    { to: "/users", label: "Users", icon: Users },
    { to: "/admin/roles", label: "Roles & permissions", icon: ShieldCheck },
    { to: "/admin/categories", label: "Ticket categories", icon: FolderCog },
    { to: "/admin/settings", label: "System settings", icon: Settings },
    { to: "/activity-logs", label: "Activity logs", icon: Activity },
    { to: "/reports", label: "Reports", icon: BarChart3 },
    ...common,
  ];
}

function getSectionTitle(pathname: string): string {
  if (pathname === "/admin") return "System overview";
  if (pathname.startsWith("/admin/roles")) return "Roles & permissions";
  if (pathname.startsWith("/admin/categories")) return "Ticket categories";
  if (pathname.startsWith("/admin/settings")) return "System settings";
  if (pathname.startsWith("/activity-logs")) return "Activity logs";
  if (pathname.startsWith("/reports")) return "Dashboard & reports";
  if (pathname.startsWith("/users")) return "User management";
  if (pathname.startsWith("/notifications")) return "Notifications";
  if (pathname.startsWith("/profile")) return "My profile";
  return "Support tickets";
}
