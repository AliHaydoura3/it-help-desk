import { FolderCog, LoaderCircle, RefreshCw, Save } from "lucide-react";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { getApiErrorMessage } from "@/features/users/utils/getApiErrorMessage";
import { useAdminTicketCategories, useUpdateAdminTicketCategory } from "../hooks/useAdmin";
import type { TicketCategorySetting } from "../types/admin";
import { PageHeading } from "./AdminOverview";

export function CategoryManagement() {
  const query = useAdminTicketCategories();
  return <>
    <PageHeading eyebrow="Ticket configuration" title="Ticket categories" description="Control the labels, descriptions, ordering, and availability shown when users create or recategorize tickets." action={<Button onClick={() => query.refetch()} variant="outline"><RefreshCw />Refresh</Button>} />
    <div className="mt-5 rounded-xl border border-amber-500/25 bg-amber-500/[0.06] p-4 text-sm text-muted-foreground"><strong className="text-foreground">Historical data is preserved.</strong> Deactivating a category removes it from new selections but does not alter existing tickets or report history.</div>
    {query.isLoading ? <State text="Loading categories…" /> : query.isError || !query.data ? <State text="Categories could not be loaded" /> : <div className="mt-6 grid gap-4 lg:grid-cols-2">{query.data.map((category) => <CategoryEditor category={category} key={category.category} />)}</div>}
  </>;
}

function CategoryEditor({ category }: { category: TicketCategorySetting }) {
  const mutation = useUpdateAdminTicketCategory();
  const [displayName, setDisplayName] = useState(category.displayName);
  const [description, setDescription] = useState(category.description);
  const [sortOrder, setSortOrder] = useState(category.sortOrder);
  const [isActive, setIsActive] = useState(category.isActive);
  useEffect(() => { setDisplayName(category.displayName); setDescription(category.description); setSortOrder(category.sortOrder); setIsActive(category.isActive); }, [category]);
  const dirty = displayName !== category.displayName || description !== category.description || sortOrder !== category.sortOrder || isActive !== category.isActive;

  async function save() {
    try {
      await mutation.mutateAsync({ category: category.category, displayName, description, sortOrder, isActive });
      toast.success(`${displayName} category updated.`);
    } catch (error) { toast.error(getApiErrorMessage(error, "Unable to update this category.")); }
  }

  return <Card><CardContent className="p-5"><div className="flex items-center justify-between gap-4"><div className="flex items-center gap-3"><div className="flex size-9 items-center justify-center rounded-lg bg-primary/10 text-primary"><FolderCog className="size-4" /></div><div><h2 className="font-semibold">{category.category === "AccessRequest" ? "Access Request" : category.category}</h2><p className="text-xs text-muted-foreground">Stable system key</p></div></div><label className="flex cursor-pointer items-center gap-2 text-sm font-medium"><input checked={isActive} className="size-4 accent-primary" onChange={(event) => setIsActive(event.target.checked)} type="checkbox" />Active</label></div><div className="mt-5 space-y-4"><div className="space-y-2"><Label htmlFor={`${category.category}-name`}>Display name</Label><Input id={`${category.category}-name`} maxLength={80} onChange={(event) => setDisplayName(event.target.value)} value={displayName} /></div><div className="space-y-2"><Label htmlFor={`${category.category}-description`}>Description</Label><textarea id={`${category.category}-description`} maxLength={300} className="min-h-20 w-full rounded-lg border border-input bg-transparent px-3 py-2 text-sm outline-none focus:border-ring focus:ring-3 focus:ring-ring/30" onChange={(event) => setDescription(event.target.value)} value={description} /></div><div className="flex items-end justify-between gap-4"><div className="w-28 space-y-2"><Label htmlFor={`${category.category}-order`}>Sort order</Label><Input id={`${category.category}-order`} min={0} max={1000} onChange={(event) => setSortOrder(Number(event.target.value))} type="number" value={sortOrder} /></div><Button disabled={!dirty || mutation.isPending || !displayName.trim()} onClick={save}>{mutation.isPending ? <LoaderCircle className="animate-spin" /> : <Save />}Save</Button></div></div></CardContent></Card>;
}

function State({ text }: { text: string }) { return <div className="mt-6 flex min-h-72 items-center justify-center rounded-xl border bg-card text-muted-foreground">{text}</div>; }
