using System.IO.Compression;
using System.Text;
using HelpDesk.Application.Abstractions.Attachments;

namespace HelpDesk.Infrastructure.Attachments;

internal static class AttachmentContentInspector
{
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private static readonly byte[] ZipSignature = [0x50, 0x4B];

    public static async Task<bool> IsValidAsync(
        string path,
        AttachmentContentKind contentKind,
        CancellationToken cancellationToken)
    {
        try
        {
            return contentKind switch
            {
                AttachmentContentKind.Png => await StartsWithAsync(path, PngSignature, cancellationToken),
                AttachmentContentKind.Jpeg => await StartsWithAsync(path, JpegSignature, cancellationToken),
                AttachmentContentKind.Pdf => await StartsWithAsync(path, PdfSignature, cancellationToken),
                AttachmentContentKind.PlainText or AttachmentContentKind.Csv =>
                    await IsUtf8TextAsync(path, cancellationToken),
                AttachmentContentKind.Zip => await IsZipAsync(path, null, cancellationToken),
                AttachmentContentKind.WordOpenXml =>
                    await IsZipAsync(path, "word/document.xml", cancellationToken),
                AttachmentContentKind.ExcelOpenXml =>
                    await IsZipAsync(path, "xl/workbook.xml", cancellationToken),
                _ => false
            };
        }
        catch (InvalidDataException)
        {
            return false;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    private static async Task<bool> StartsWithAsync(
        string path,
        byte[] signature,
        CancellationToken cancellationToken)
    {
        await using var stream = OpenRead(path);
        var header = new byte[signature.Length];
        return await stream.ReadAtLeastAsync(
            header,
            signature.Length,
            throwOnEndOfStream: false,
            cancellationToken) == signature.Length &&
            header.AsSpan().SequenceEqual(signature);
    }

    private static async Task<bool> IsUtf8TextAsync(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = OpenRead(path);
        using var reader = new StreamReader(
            stream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 4096,
            leaveOpen: false);
        var buffer = new char[4096];
        while (true)
        {
            var read = await reader.ReadAsync(buffer, cancellationToken);
            if (read == 0) return true;
            if (buffer.AsSpan(0, read).Contains('\0')) return false;
        }
    }

    private static async Task<bool> IsZipAsync(
        string path,
        string? requiredEntry,
        CancellationToken cancellationToken)
    {
        if (!await StartsWithAsync(path, ZipSignature, cancellationToken)) return false;
        await using var stream = OpenRead(path);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
        if (archive.Entries.Count == 0) return false;
        if (requiredEntry is null) return true;

        return archive.GetEntry("[Content_Types].xml") is not null &&
            archive.GetEntry(requiredEntry) is not null;
    }

    private static FileStream OpenRead(string path) =>
        new(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
}
