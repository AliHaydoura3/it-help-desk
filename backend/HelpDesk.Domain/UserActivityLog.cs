namespace HelpDesk.Domain;

public sealed class UserActivityLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string? ResourceId { get; set; }
    public string? IpAddress { get; set; }
    public bool Succeeded { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}
