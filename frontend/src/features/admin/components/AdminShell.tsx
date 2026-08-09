import { useState, type ReactNode } from "react";
import {
  Activity, BarChart3, Bell, FolderCog, Gauge, LogOut, Menu,
  Settings, ShieldCheck, TicketCheck, UserRound, Users, X,
} from "lucide-react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { logoutSession } from "@/features/auth/api/logout";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { NotificationBell } from "@/features/communication/components/NotificationBell";
import { cn } from "@/lib/utils";

const navigation = [
  { to: "/admin", label: "System overview", icon: Gauge, exact: true },
  { to: "/users", label: "Users", icon: Users },
  { to: "/admin/roles", label: "Roles & permissions", icon: ShieldCheck },
  { to: "/admin/categories", label: "Ticket categories", icon: FolderCog },
  { to: "/admin/settings", label: "System settings", icon: Settings },
  { to: "/activity-logs", label: "Activity logs", icon: Activity },
  { to: "/reports", label: "Reports", icon: BarChart3 },
  { to: "/tickets", label: "Tickets", icon: TicketCheck },
  { to: "/notifications", label: "Notifications", icon: Bell },
];

export function AdminShell({ children }: { children: ReactNode }) {
  const auth = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);
  const email = auth.user?.email ?? "Administrator";
  const initials = email.split("@")[0].split(/[._-]/).map((part) => part[0] ?? "").join("").slice(0, 2).toUpperCase();

  async function logout() {
    try { await logoutSession(); } catch { /* local logout still completes */ }
    auth.logout();
    navigate("/login");
  }

  const nav = (
    <>
      <div className="flex h-16 items-center gap-3 border-b px-5">
        <div className="flex size-9 items-center justify-center rounded-xl bg-primary text-primary-foreground"><TicketCheck className="size-5" /></div>
        <div><p className="font-semibold leading-none">IT Help Desk</p><p className="mt-1 text-xs text-muted-foreground">Administration</p></div>
      </div>
      <nav className="flex-1 overflow-y-auto px-3 py-4">
        <p className="mb-2 px-3 text-[11px] font-semibold uppercase tracking-[0.16em] text-muted-foreground">Admin panel</p>
        {navigation.map(({ to, label, icon: Icon, exact }) => {
          const active = exact ? location.pathname === to : location.pathname === to || location.pathname.startsWith(`${to}/`);
          return <Link key={to} onClick={() => setMobileOpen(false)} to={to} className={cn(
            "mb-1 flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm transition-colors",
            active ? "bg-primary font-medium text-primary-foreground shadow-sm" : "text-muted-foreground hover:bg-muted hover:text-foreground",
          )}><Icon className="size-4" />{label}</Link>;
        })}
      </nav>
      <div className="border-t p-3">
        <Link to="/profile" className="flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"><UserRound className="size-4" />My profile</Link>
        <button onClick={logout} className="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"><LogOut className="size-4" />Sign out</button>
      </div>
    </>
  );

  return <div className="min-h-screen bg-muted/35">
    <aside className="fixed inset-y-0 left-0 z-40 hidden w-64 flex-col border-r bg-card lg:flex">{nav}</aside>
    {mobileOpen && <div className="fixed inset-0 z-50 bg-foreground/30 lg:hidden" onClick={() => setMobileOpen(false)}>
      <aside className="flex h-full w-72 flex-col bg-card shadow-xl" onClick={(event) => event.stopPropagation()}>
        <div className="relative flex flex-1 flex-col">{nav}<Button aria-label="Close navigation" className="absolute right-2 top-2" onClick={() => setMobileOpen(false)} size="icon" variant="ghost"><X /></Button></div>
      </aside>
    </div>}
    <div className="lg:pl-64">
      <header className="sticky top-0 z-30 flex h-16 items-center border-b bg-background/90 px-4 backdrop-blur sm:px-6 lg:px-8">
        <Button aria-label="Open navigation" className="mr-3 lg:hidden" onClick={() => setMobileOpen(true)} size="icon" variant="ghost"><Menu /></Button>
        <p className="min-w-0 flex-1 truncate text-sm font-medium">System administration</p>
        <div className="flex items-center gap-3"><NotificationBell /><div className="hidden text-right sm:block"><p className="max-w-52 truncate text-sm font-medium">{email}</p><p className="text-xs text-muted-foreground">Administrator</p></div><div className="flex size-9 items-center justify-center rounded-full bg-primary text-xs font-semibold text-primary-foreground">{initials || "A"}</div></div>
      </header>
      <main className="mx-auto max-w-7xl px-4 py-7 sm:px-6 lg:px-8 lg:py-9">{children}</main>
    </div>
  </div>;
}
