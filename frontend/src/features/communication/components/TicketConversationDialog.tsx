import { AtSign, LoaderCircle, MessageCircle, Paperclip, Reply, Send, X } from "lucide-react";
import { useEffect, useMemo, useState } from "react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";
import type { Ticket } from "@/features/tickets/types/ticket";
import type { AuthUser } from "@/features/auth/types/auth";
import { canCommentOnTicket } from "@/features/tickets/utils/ticketPermissions";
import { getApiErrorMessage } from "@/features/users/utils/getApiErrorMessage";
import { useAddTicketComment, useMentionableAgents, useTicketComments } from "../hooks/useTicketConversation";
import type { MentionableAgent, TicketComment } from "../types/communication";
import { formatRelativeTime, getFullName, getInitials } from "../utils/communicationPresentation";
import { CommunicationPagination } from "./CommunicationPagination";
import { TicketAttachmentsPanel } from "@/features/attachments/components/TicketAttachmentsPanel";

const PAGE_SIZE = 10;

export function TicketConversationDialog({ ticket, user, onClose }: { ticket: Ticket | null; user: AuthUser | null; onClose: () => void }) {
  const [page, setPage] = useState(1);
  const [content, setContent] = useState("");
  const [replyingTo, setReplyingTo] = useState<TicketComment | null>(null);
  const [mentionPickerOpen, setMentionPickerOpen] = useState(false);
  const [mentionSearch, setMentionSearch] = useState("");
  const [debouncedMentionSearch, setDebouncedMentionSearch] = useState("");
  const [mentionedAgents, setMentionedAgents] = useState<MentionableAgent[]>([]);
  const [latestPageSelected, setLatestPageSelected] = useState(false);
  const [activeTab, setActiveTab] = useState<"comments" | "attachments">("comments");
  const commentsQuery = useTicketComments(ticket?.id ?? null, page, PAGE_SIZE);
  const agentsQuery = useMentionableAgents(ticket?.id ?? null, debouncedMentionSearch, mentionPickerOpen);
  const addComment = useAddTicketComment(ticket?.id ?? null);
  const commentable = ticket !== null && canCommentOnTicket(user, ticket);

  useEffect(() => {
    const timeout = window.setTimeout(() => setDebouncedMentionSearch(mentionSearch.trim()), 250);
    return () => window.clearTimeout(timeout);
  }, [mentionSearch]);

  useEffect(() => {
    if (!ticket) return;
    setPage(1);
    setContent("");
    setReplyingTo(null);
    setMentionPickerOpen(false);
    setMentionSearch("");
    setMentionedAgents([]);
    setLatestPageSelected(false);
    setActiveTab("comments");
  }, [ticket]);

  useEffect(() => {
    const totalPages = commentsQuery.data?.totalPages;
    if (totalPages === undefined || latestPageSelected) return;
    setLatestPageSelected(true);
    if (totalPages > 1) setPage(totalPages);
  }, [commentsQuery.data?.totalPages, latestPageSelected]);

  const availableAgents = useMemo(() => {
    const selected = new Set(mentionedAgents.map((agent) => agent.id));
    return (agentsQuery.data ?? []).filter((agent) => !selected.has(agent.id));
  }, [agentsQuery.data, mentionedAgents]);

  if (!ticket) return null;

  function addMention(agent: MentionableAgent) {
    setMentionedAgents((current) => [...current, agent]);
    setMentionSearch("");
  }

  async function submitComment(event: React.FormEvent) {
    event.preventDefault();
    const trimmedContent = content.trim();
    if (!trimmedContent) return;

    try {
      const createsNewPage = (commentsQuery.data?.totalCount ?? 0) > 0 &&
        (commentsQuery.data?.totalCount ?? 0) % PAGE_SIZE === 0;
      await addComment.mutateAsync({
        content: trimmedContent,
        parentCommentId: replyingTo?.id ?? null,
        mentionedAgentIds: mentionedAgents.map((agent) => agent.id),
      });
      if (createsNewPage) setPage((current) => current + 1);
      setContent("");
      setReplyingTo(null);
      setMentionedAgents([]);
      setMentionPickerOpen(false);
      toast.success(replyingTo ? "Reply posted." : "Comment posted.");
    } catch (error) {
      toast.error(getApiErrorMessage(error, "Unable to post your comment."));
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-foreground/35 p-4 backdrop-blur-sm">
      <section className="flex max-h-[92vh] w-full max-w-3xl flex-col overflow-hidden rounded-2xl bg-card shadow-2xl">
        <header className="flex items-start justify-between border-b px-5 py-4 sm:px-6 sm:py-5">
          <div>
            <div className="flex items-center gap-2">
              <MessageCircle className="size-5 text-muted-foreground" />
              <h2 className="text-xl font-semibold">Ticket activity</h2>
            </div>
            <p className="mt-1 text-sm text-muted-foreground">{ticket.referenceNumber} · {ticket.title}</p>
          </div>
          <Button aria-label="Close conversation" onClick={onClose} size="icon" variant="ghost"><X /></Button>
        </header>

        <nav aria-label="Ticket activity sections" className="flex gap-1 border-b px-4 py-2">
          <button aria-current={activeTab === "comments" ? "page" : undefined} className={cn("flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors", activeTab === "comments" ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:bg-muted hover:text-foreground")} onClick={() => setActiveTab("comments")}><MessageCircle className="size-4" /> Comments</button>
          <button aria-current={activeTab === "attachments" ? "page" : undefined} className={cn("flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors", activeTab === "attachments" ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:bg-muted hover:text-foreground")} onClick={() => setActiveTab("attachments")}><Paperclip className="size-4" /> Attachments</button>
        </nav>

        {activeTab === "attachments" ? (
          <TicketAttachmentsPanel ticket={ticket} user={user} />
        ) : (
          <>

        <div className="min-h-64 flex-1 overflow-y-auto bg-muted/20">
          {commentsQuery.isLoading ? (
            <div className="flex min-h-72 items-center justify-center"><LoaderCircle className="animate-spin text-muted-foreground" /></div>
          ) : commentsQuery.isError ? (
            <div className="flex min-h-72 flex-col items-center justify-center px-6 text-center">
              <p className="font-medium">Could not load this conversation</p>
              <p className="mt-1 text-sm text-muted-foreground">Check your connection and try again.</p>
              <Button className="mt-4" onClick={() => commentsQuery.refetch()} variant="outline">Try again</Button>
            </div>
          ) : commentsQuery.data?.items.length === 0 ? (
            <div className="flex min-h-72 flex-col items-center justify-center px-6 text-center">
              <div className="flex size-12 items-center justify-center rounded-full bg-primary/8 text-primary"><MessageCircle className="size-5" /></div>
              <p className="mt-4 font-medium">Start the conversation</p>
              <p className="mt-1 max-w-sm text-sm text-muted-foreground">Add context, ask a follow-up question, or tag a support agent.</p>
            </div>
          ) : (
            <div className="space-y-3 p-4 sm:p-6">
              {commentsQuery.data?.items.map((comment) => (
                <CommentItem canReply={commentable} comment={comment} key={comment.id} onReply={setReplyingTo} />
              ))}
            </div>
          )}
        </div>

        {commentsQuery.data && commentsQuery.data.totalCount > 0 && (
          <CommunicationPagination
            disabled={commentsQuery.isFetching}
            noun="comments"
            onPageChange={setPage}
            pageNumber={commentsQuery.data.pageNumber}
            pageSize={commentsQuery.data.pageSize}
            totalCount={commentsQuery.data.totalCount}
            totalPages={commentsQuery.data.totalPages}
          />
        )}

        {commentable ? <form className="border-t bg-card p-4 sm:px-6" onSubmit={submitComment}>
          {replyingTo && (
            <div className="mb-3 flex items-center gap-2 rounded-lg bg-muted px-3 py-2 text-xs">
              <Reply className="size-3.5 text-muted-foreground" />
              <span className="min-w-0 flex-1 truncate">Replying to {getFullName(replyingTo.author)}: {replyingTo.content}</span>
              <button aria-label="Cancel reply" className="text-muted-foreground hover:text-foreground" onClick={() => setReplyingTo(null)} type="button"><X className="size-3.5" /></button>
            </div>
          )}

          {mentionedAgents.length > 0 && (
            <div className="mb-3 flex flex-wrap gap-2">
              {mentionedAgents.map((agent) => (
                <span className="inline-flex items-center gap-1 rounded-full bg-primary/8 px-2 py-1 text-xs font-medium text-primary" key={agent.id}>
                  @{getFullName(agent)}
                  <button aria-label={`Remove ${getFullName(agent)}`} onClick={() => setMentionedAgents((current) => current.filter((item) => item.id !== agent.id))} type="button"><X className="size-3" /></button>
                </span>
              ))}
            </div>
          )}

          {mentionPickerOpen && (
            <div className="relative mb-3 rounded-xl border bg-background p-3 shadow-sm">
              <Input autoFocus className="h-8" onChange={(event) => setMentionSearch(event.target.value)} placeholder="Search support agents..." value={mentionSearch} />
              <div className="mt-2 max-h-36 overflow-y-auto">
                {agentsQuery.isLoading ? (
                  <div className="flex justify-center py-5"><LoaderCircle className="size-4 animate-spin text-muted-foreground" /></div>
                ) : availableAgents.length === 0 ? (
                  <p className="py-4 text-center text-xs text-muted-foreground">No matching support agents.</p>
                ) : availableAgents.map((agent) => (
                  <button className="flex w-full items-center gap-3 rounded-lg px-2 py-2 text-left hover:bg-muted" key={agent.id} onClick={() => addMention(agent)} type="button">
                    <div className="flex size-8 items-center justify-center rounded-full bg-muted text-[11px] font-semibold">{getInitials(agent.firstName, agent.lastName)}</div>
                    <div className="min-w-0"><p className="truncate text-sm font-medium">{getFullName(agent)}</p><p className="truncate text-xs text-muted-foreground">{agent.email}</p></div>
                  </button>
                ))}
              </div>
            </div>
          )}

          <textarea
            className="min-h-24 w-full resize-y rounded-xl border border-input bg-background px-3 py-2 text-sm outline-none transition focus:border-ring focus:ring-3 focus:ring-ring/30"
            maxLength={4000}
            onChange={(event) => setContent(event.target.value)}
            placeholder={replyingTo ? "Write a reply..." : "Write a comment..."}
            value={content}
          />
          <div className="mt-2 flex items-center justify-between gap-3">
            <Button aria-expanded={mentionPickerOpen} onClick={() => setMentionPickerOpen((open) => !open)} type="button" variant="ghost">
              <AtSign /> Mention agent
            </Button>
            <div className="flex items-center gap-3">
              <span className="hidden text-xs text-muted-foreground sm:inline">{content.length}/4000</span>
              <Button disabled={addComment.isPending || content.trim().length === 0} type="submit">
                {addComment.isPending ? <LoaderCircle className="animate-spin" /> : <Send />} Post
              </Button>
            </div>
          </div>
        </form> : (
          <div className="border-t bg-card px-6 py-4 text-center text-sm text-muted-foreground">
            {ticket.isCancelled || ticket.status === "Closed"
              ? `This conversation is read-only because the ticket is ${ticket.isCancelled ? "cancelled" : "closed"}.`
              : "Your role has read-only access to this ticket conversation."}
          </div>
        )}
          </>
        )}
      </section>
    </div>
  );
}

function CommentItem({ comment, canReply, onReply }: { comment: TicketComment; canReply: boolean; onReply: (comment: TicketComment) => void }) {
  return (
    <article className={cn("rounded-xl border bg-card p-4 shadow-xs", comment.parentCommentId && "ml-5 border-l-2 border-l-primary/35 sm:ml-9")}>
      <div className="flex gap-3">
        <div className="flex size-9 shrink-0 items-center justify-center rounded-full bg-primary/8 text-xs font-semibold text-primary">
          {getInitials(comment.author.firstName, comment.author.lastName)}
        </div>
        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-x-2 gap-y-1">
            <p className="text-sm font-medium">{getFullName(comment.author)}</p>
            <span className="text-xs text-muted-foreground" title={new Date(comment.createdAtUtc).toLocaleString()}>{formatRelativeTime(comment.createdAtUtc)}</span>
            {comment.parentCommentId && <span className="rounded-full bg-muted px-2 py-0.5 text-[10px] font-medium text-muted-foreground">Reply</span>}
          </div>
          <p className="mt-2 whitespace-pre-wrap break-words text-sm leading-6">{comment.content}</p>
          {comment.mentions.length > 0 && (
            <div className="mt-3 flex flex-wrap gap-1.5">
              {comment.mentions.map((agent) => <span className="rounded-full bg-primary/8 px-2 py-1 text-[11px] font-medium text-primary" key={agent.id}>@{getFullName(agent)}</span>)}
            </div>
          )}
          {canReply && <Button className="mt-2 -ml-2 text-muted-foreground" onClick={() => onReply(comment)} size="sm" variant="ghost"><Reply /> Reply</Button>}
        </div>
      </div>
    </article>
  );
}
