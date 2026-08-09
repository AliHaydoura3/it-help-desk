namespace HelpDesk.Domain;

public sealed class TicketComment
{
    private readonly List<TicketCommentMention> _mentions = [];

    private TicketComment() { }

    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public Guid AuthorUserId { get; private set; }
    public Guid? ParentCommentId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<TicketCommentMention> Mentions => _mentions;

    public static TicketComment Create(
        Guid ticketId,
        Guid authorUserId,
        Guid? parentCommentId,
        string content,
        IEnumerable<Guid> mentionedAgentIds,
        DateTime createdAtUtc)
    {
        if (ticketId == Guid.Empty)
            throw new DomainRuleException("A comment must belong to a ticket.");
        if (authorUserId == Guid.Empty)
            throw new DomainRuleException("A comment author is required.");
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainRuleException("Comment content is required.");

        var comment = new TicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            AuthorUserId = authorUserId,
            ParentCommentId = parentCommentId,
            Content = content.Trim(),
            CreatedAtUtc = createdAtUtc
        };

        foreach (var agentId in mentionedAgentIds.Distinct())
            comment._mentions.Add(TicketCommentMention.Create(comment.Id, agentId));

        return comment;
    }
}

public sealed class TicketCommentMention
{
    private TicketCommentMention() { }

    public Guid TicketCommentId { get; private set; }
    public Guid MentionedUserId { get; private set; }

    internal static TicketCommentMention Create(Guid commentId, Guid mentionedUserId) =>
        new() { TicketCommentId = commentId, MentionedUserId = mentionedUserId };
}
