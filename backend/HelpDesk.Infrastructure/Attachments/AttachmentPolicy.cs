using HelpDesk.Application.Abstractions.Attachments;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Attachments;

public sealed class AttachmentPolicy : IAttachmentPolicy
{
    private static readonly IReadOnlyDictionary<string, AttachmentDefinition> KnownTypes =
        new Dictionary<string, AttachmentDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            [".png"] = new("image/png", AttachmentContentKind.Png, ["image/png"]),
            [".jpg"] = new("image/jpeg", AttachmentContentKind.Jpeg, ["image/jpeg"]),
            [".jpeg"] = new("image/jpeg", AttachmentContentKind.Jpeg, ["image/jpeg"]),
            [".pdf"] = new("application/pdf", AttachmentContentKind.Pdf, ["application/pdf"]),
            [".txt"] = new("text/plain", AttachmentContentKind.PlainText, ["text/plain"]),
            [".log"] = new("text/plain", AttachmentContentKind.PlainText, ["text/plain", "application/octet-stream"]),
            [".csv"] = new("text/csv", AttachmentContentKind.Csv, ["text/csv", "application/csv", "text/plain"]),
            [".zip"] = new("application/zip", AttachmentContentKind.Zip, ["application/zip", "application/x-zip-compressed", "application/octet-stream"]),
            [".docx"] = new(
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                AttachmentContentKind.WordOpenXml,
                ["application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/octet-stream"]),
            [".xlsx"] = new(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                AttachmentContentKind.ExcelOpenXml,
                ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/octet-stream"])
        };

    private readonly IReadOnlyDictionary<string, AttachmentDefinition> _allowedTypes;

    public AttachmentPolicy(IOptions<AttachmentOptions> options)
    {
        var configured = options.Value;
        MaximumFileSizeBytes = configured.MaximumFileSizeBytes;
        MaximumFilesPerTicket = configured.MaximumFilesPerTicket;
        _allowedTypes = configured.AllowedExtensions
            .Select(NormalizeExtension)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                extension => extension,
                extension => KnownTypes[extension],
                StringComparer.OrdinalIgnoreCase);
        SupportedExtensions = _allowedTypes.Keys.OrderBy(value => value).ToArray();
        SupportedFileTypes = _allowedTypes
            .OrderBy(pair => pair.Key)
            .Select(pair => new AttachmentFileType(
                pair.Key,
                pair.Value.CanonicalContentType,
                pair.Value.ContentKind))
            .ToArray();
    }

    public long MaximumFileSizeBytes { get; }
    public int MaximumFilesPerTicket { get; }
    public IReadOnlyCollection<string> SupportedExtensions { get; }
    public IReadOnlyCollection<AttachmentFileType> SupportedFileTypes { get; }

    public AttachmentFileType? Match(string fileName, string declaredContentType)
    {
        var extension = NormalizeExtension(Path.GetExtension(fileName));
        if (!_allowedTypes.TryGetValue(extension, out var definition)) return null;

        var mediaType = declaredContentType
            .Split(';', 2)[0]
            .Trim();
        if (!definition.AcceptedContentTypes.Contains(mediaType, StringComparer.OrdinalIgnoreCase))
            return null;

        return new AttachmentFileType(
            extension,
            definition.CanonicalContentType,
            definition.ContentKind);
    }

    public static bool IsKnownExtension(string extension) =>
        KnownTypes.ContainsKey(NormalizeExtension(extension));

    private static string NormalizeExtension(string extension)
    {
        var normalized = extension.Trim().ToLowerInvariant();
        if (normalized.Length == 0) return normalized;
        return normalized[0] == '.' ? normalized : $".{normalized}";
    }

    private sealed record AttachmentDefinition(
        string CanonicalContentType,
        AttachmentContentKind ContentKind,
        IReadOnlyCollection<string> AcceptedContentTypes);
}
