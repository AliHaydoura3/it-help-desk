import { LoaderCircle, Save, Settings } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { getApiErrorMessage } from "@/features/users/utils/getApiErrorMessage";
import { useSystemSettings, useUpdateSystemSettings } from "../hooks/useAdmin";
import type { UpdateSystemSettingsRequest } from "../types/admin";
import { PageHeading } from "./AdminOverview";

const empty: UpdateSystemSettingsRequest = { organizationName: "", supportEmail: "", automaticAssignmentEnabled: true, emailNotificationsEnabled: true, maximumOpenTicketsPerEmployee: 25 };

export function SystemSettingsPanel() {
  const query = useSystemSettings();
  const mutation = useUpdateSystemSettings();
  const [form, setForm] = useState(empty);
  useEffect(() => { if (query.data) setForm({ organizationName: query.data.organizationName, supportEmail: query.data.supportEmail, automaticAssignmentEnabled: query.data.automaticAssignmentEnabled, emailNotificationsEnabled: query.data.emailNotificationsEnabled, maximumOpenTicketsPerEmployee: query.data.maximumOpenTicketsPerEmployee }); }, [query.data]);

  async function save() {
    try { await mutation.mutateAsync(form); toast.success("System settings updated."); }
    catch (error) { toast.error(getApiErrorMessage(error, "Unable to update system settings.")); }
  }

  return <>
    <PageHeading eyebrow="Configuration" title="System settings" description="Manage operational behavior centrally. Changes affect future actions and do not rewrite existing ticket or notification history." />
    {query.isLoading ? <div className="mt-7 text-sm text-muted-foreground">Loading settings…</div> : query.isError ? <div className="mt-7 text-sm text-destructive">Settings could not be loaded.</div> : <div className="mt-7 grid gap-6 xl:grid-cols-[1fr_1.1fr]">
      <Card><CardContent className="p-5"><div className="flex items-center gap-3"><div className="flex size-10 items-center justify-center rounded-xl bg-primary/10 text-primary"><Settings className="size-5" /></div><div><h2 className="font-semibold">Organization</h2><p className="text-sm text-muted-foreground">Identity and support contact details.</p></div></div><div className="mt-6 space-y-4"><Field label="Organization name"><Input maxLength={120} onChange={(event) => setForm({ ...form, organizationName: event.target.value })} value={form.organizationName} /></Field><Field label="Support email"><Input maxLength={256} onChange={(event) => setForm({ ...form, supportEmail: event.target.value })} type="email" value={form.supportEmail} /></Field><Field label="Maximum active tickets per employee"><Input min={1} max={1000} onChange={(event) => setForm({ ...form, maximumOpenTicketsPerEmployee: Number(event.target.value) })} type="number" value={form.maximumOpenTicketsPerEmployee} /></Field></div></CardContent></Card>
      <Card><CardContent className="p-5"><h2 className="font-semibold">Operational controls</h2><p className="mt-1 text-sm text-muted-foreground">Enable or pause automated capabilities without redeploying the application.</p><div className="mt-6 divide-y"><Toggle title="Automatic ticket assignment" description="Allows administrators and support agents to assign a ticket to the least-loaded active agent." checked={form.automaticAssignmentEnabled} onChange={(checked) => setForm({ ...form, automaticAssignmentEnabled: checked })} /><Toggle title="Email notifications" description="Queues email delivery for new alerts. In-app and real-time notifications continue when disabled." checked={form.emailNotificationsEnabled} onChange={(checked) => setForm({ ...form, emailNotificationsEnabled: checked })} /></div><div className="mt-6 flex justify-end"><Button disabled={mutation.isPending || !form.organizationName.trim() || !form.supportEmail.trim() || form.maximumOpenTicketsPerEmployee < 1} onClick={save}>{mutation.isPending ? <LoaderCircle className="animate-spin" /> : <Save />}Save settings</Button></div></CardContent></Card>
    </div>}
    {query.data && <p className="mt-4 text-xs text-muted-foreground">Last updated {new Date(query.data.updatedAtUtc).toLocaleString()}.</p>}
  </>;
}

function Field({ label, children }: { label: string; children: React.ReactNode }) { return <div className="space-y-2"><Label>{label}</Label>{children}</div>; }
function Toggle({ title, description, checked, onChange }: { title: string; description: string; checked: boolean; onChange: (checked: boolean) => void }) { return <label className="flex cursor-pointer items-start justify-between gap-5 py-5 first:pt-0"><div><p className="text-sm font-medium">{title}</p><p className="mt-1 text-xs leading-relaxed text-muted-foreground">{description}</p></div><input aria-label={title} checked={checked} className="mt-1 size-5 shrink-0 accent-primary" onChange={(event) => onChange(event.target.checked)} type="checkbox" /></label>; }
