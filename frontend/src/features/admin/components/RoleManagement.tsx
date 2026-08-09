import { Check, RefreshCw, ShieldCheck, Users } from "lucide-react";
import { Link } from "react-router-dom";
import { Button, buttonVariants } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useAdminRoles } from "../hooks/useAdmin";
import { PageHeading } from "./AdminOverview";

export function RoleManagement() {
  const query = useAdminRoles();
  return <>
    <PageHeading eyebrow="Access control" title="Roles & permissions" description="The help desk uses exactly four fixed, single-assignment roles. Assign roles to users without creating overlapping or ambiguous permission sets." action={<Link className={buttonVariants()} to="/users"><Users />Assign user roles</Link>} />
    {query.isLoading ? <State text="Loading role policy…" /> : query.isError || !query.data ? <State text="Role policy could not be loaded" action={<Button onClick={() => query.refetch()} variant="outline"><RefreshCw />Try again</Button>} /> : <div className="mt-7 grid gap-5 lg:grid-cols-2">{query.data.map((role) => <Card key={role.name}><CardContent className="p-5"><div className="flex items-start justify-between gap-4"><div className="flex gap-3"><div className="flex size-10 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary"><ShieldCheck className="size-5" /></div><div><h2 className="font-semibold">{role.displayName}</h2><p className="mt-1 text-sm text-muted-foreground">{role.description}</p></div></div><span className="whitespace-nowrap rounded-full bg-muted px-2.5 py-1 text-xs font-medium">{role.assignedUserCount} users</span></div><div className="mt-5 border-t pt-4"><p className="mb-3 text-xs font-semibold uppercase tracking-wider text-muted-foreground">Effective permissions</p><div className="grid gap-2 sm:grid-cols-2">{role.permissions.map((permission) => <div className="flex items-start gap-2 text-sm" key={permission}><Check className="mt-0.5 size-3.5 shrink-0 text-emerald-600" /><span>{humanize(permission)}</span></div>)}</div></div></CardContent></Card>)}</div>}
    <div className="mt-6 rounded-xl border border-primary/20 bg-primary/[0.04] p-4 text-sm"><strong>Single-role policy:</strong> an account has one role only. The administrator role already includes every permission; support agents and managers inherit employee ticket basics plus their role-specific capabilities.</div>
  </>;
}

function humanize(value: string) { return value.replace(/([a-z])([A-Z])/g, "$1 $2"); }
function State({ text, action }: { text: string; action?: React.ReactNode }) { return <div className="mt-7 flex min-h-72 flex-col items-center justify-center gap-4 rounded-xl border bg-card"><ShieldCheck className="size-8 text-muted-foreground" /><p>{text}</p>{action}</div>; }
