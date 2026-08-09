namespace HelpDesk.Application.Abstractions.Attachments;

public interface IAttachmentStorage
{
    Task<StoredAttachmentFile> StoreAsync(
        Stream content,
        AttachmentFileType fileType,
        long maximumSizeBytes,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken);

    Task DeleteIfExistsAsync(
        string storageKey,
        CancellationToken cancellationToken);
}

public sealed record StoredAttachmentFile(
    string StorageKey,
    long SizeBytes,
    string Sha256Hash);
