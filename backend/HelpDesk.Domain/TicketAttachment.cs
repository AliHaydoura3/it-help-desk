namespace HelpDesk.Domain;

public sealed class TicketAttachment
{
    private TicketAttachment() { }

    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public Guid UploadedByUserId { get; private set; }
    public string OriginalFileName { get; private set; } = string.Empty;
    public string StorageKey { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string Extension { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string Sha256Hash { get; private set; } = string.Empty;
    public DateTime UploadedAtUtc { get; private set; }

    internal static TicketAttachment Create(
        Guid ticketId,
        Guid uploadedByUserId,
        string originalFileName,
        string storageKey,
        string contentType,
        string extension,
        long sizeBytes,
        string sha256Hash,
        DateTime uploadedAtUtc)
    {
        if (ticketId == Guid.Empty)
            throw new DomainRuleException("An attachment must belong to a ticket.");
        if (uploadedByUserId == Guid.Empty)
            throw new DomainRuleException("An attachment uploader is required.");
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new DomainRuleException("An attachment file name is required.");
        if (originalFileName.Length > 255)
            throw new DomainRuleException("An attachment file name cannot exceed 255 characters.");
        if (string.IsNullOrWhiteSpace(storageKey))
            throw new DomainRuleException("An attachment storage key is required.");
        if (storageKey.Length > 500)
            throw new DomainRuleException("An attachment storage key cannot exceed 500 characters.");
        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainRuleException("An attachment content type is required.");
        if (contentType.Length > 127)
            throw new DomainRuleException("An attachment content type cannot exceed 127 characters.");
        if (string.IsNullOrWhiteSpace(extension))
            throw new DomainRuleException("An attachment extension is required.");
        if (extension.Length > 16)
            throw new DomainRuleException("An attachment extension cannot exceed 16 characters.");
        if (sizeBytes <= 0)
            throw new DomainRuleException("An attachment cannot be empty.");
        if (sha256Hash.Length != 64 || sha256Hash.Any(character => !Uri.IsHexDigit(character)))
            throw new DomainRuleException("An attachment SHA-256 hash is invalid.");

        return new TicketAttachment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            UploadedByUserId = uploadedByUserId,
            OriginalFileName = originalFileName.Trim(),
            StorageKey = storageKey,
            ContentType = contentType,
            Extension = extension,
            SizeBytes = sizeBytes,
            Sha256Hash = sha256Hash.ToUpperInvariant(),
            UploadedAtUtc = uploadedAtUtc
        };
    }
}
