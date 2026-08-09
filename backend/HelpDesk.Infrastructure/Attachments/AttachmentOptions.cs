namespace HelpDesk.Infrastructure.Attachments;

public sealed class AttachmentOptions
{
    public const string SectionName = "Attachments";
    public const long DefaultMaximumFileSizeBytes = 10 * 1024 * 1024;

    public string StorageRootPath { get; init; } = "App_Data/attachments";
    public long MaximumFileSizeBytes { get; init; } = DefaultMaximumFileSizeBytes;
    public int MaximumFilesPerTicket { get; init; } = 25;
    public string[] AllowedExtensions { get; init; } =
        [".png", ".jpg", ".jpeg", ".pdf", ".txt", ".log", ".csv", ".zip", ".docx", ".xlsx"];
}
