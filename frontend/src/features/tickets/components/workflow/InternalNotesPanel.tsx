import { LoaderCircle, MessageSquarePlus } from "lucide-react";
import { useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { getApiErrorMessage } from "@/features/users/utils/getApiErrorMessage";
import { useInternalNotes, useWorkflowMutations } from "../../hooks/useTicketWorkflow";
import type { Ticket } from "../../types/ticket";

export function InternalNotesPanel({ ticket }: { ticket: Ticket }) {
  const notes = useInternalNotes(ticket.id, true);
  const mutation = useWorkflowMutations(ticket.id).addNote;
  const [content, setContent] = useState("");
  async function submit() {
    try { await mutation.mutateAsync(content); setContent(""); toast.success("Internal note added."); }
    catch (error) { toast.error(getApiErrorMessage(error, "Unable to add the note.")); }
  }
  return <div className="space-y-5"><div className="space-y-2"><textarea className="min-h-24 w-full rounded-lg border bg-transparent px-3 py-2 text-sm outline-none focus:border-ring focus:ring-3 focus:ring-ring/30" maxLength={4000} placeholder="Write a note visible only to support staff..." value={content} onChange={(event) => setContent(event.target.value)} /><div className="flex justify-end"><Button disabled={!content.trim() || mutation.isPending} onClick={submit}>{mutation.isPending ? <LoaderCircle className="animate-spin" /> : <MessageSquarePlus />} Add note</Button></div></div><div className="space-y-3">{notes.isLoading ? <LoaderCircle className="mx-auto animate-spin" /> : notes.data?.length === 0 ? <p className="py-8 text-center text-sm text-muted-foreground">No internal notes yet.</p> : notes.data?.map((note) => <article className="rounded-xl border p-4" key={note.id}><p className="whitespace-pre-wrap text-sm leading-6">{note.content}</p><p className="mt-2 text-[11px] text-muted-foreground">{new Date(note.createdAtUtc).toLocaleString()} · Author {note.authorUserId.slice(0, 8)}</p></article>)}</div></div>;
}
