namespace HelpDesk.Application.Abstractions.Attachments;

public interface IAttachmentPolicy
{
    long MaximumFileSizeBytes { get; }
    int MaximumFilesPerTicket { get; }
    IReadOnlyCollection<string> SupportedExtensions { get; }
    IReadOnlyCollection<AttachmentFileType> SupportedFileTypes { get; }
    AttachmentFileType? Match(string fileName, string declaredContentType);
}

public sealed record AttachmentFileType(
    string Extension,
    string ContentType,
    AttachmentContentKind ContentKind);

public enum AttachmentContentKind
{
    Png,
    Jpeg,
    Pdf,
    PlainText,
    Csv,
    Zip,
    WordOpenXml,
    ExcelOpenXml
}
