using System.Buffers;
using System.Security.Cryptography;
using FluentValidation;
using FluentValidation.Results;
using HelpDesk.Application.Abstractions.Attachments;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Attachments;

public sealed class LocalAttachmentStorage : IAttachmentStorage
{
    private const int BufferSize = 81920;
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;
    private readonly string _storageRoot;

    public LocalAttachmentStorage(
        IHostEnvironment environment,
        IOptions<AttachmentOptions> options)
    {
        var configuredPath = options.Value.StorageRootPath;
        _storageRoot = Path.GetFullPath(Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(environment.ContentRootPath, configuredPath));
        var webRoot = Path.GetFullPath(
            environment is IWebHostEnvironment webEnvironment &&
            !string.IsNullOrWhiteSpace(webEnvironment.WebRootPath)
                ? webEnvironment.WebRootPath
                : Path.Combine(environment.ContentRootPath, "wwwroot"));
        if (IsAtOrBelow(_storageRoot, webRoot))
        {
            throw new InvalidOperationException(
                "Attachment storage must be outside the public web root.");
        }
        Directory.CreateDirectory(_storageRoot);
        HardenDirectoryPermissions(_storageRoot);
    }

    public async Task<StoredAttachmentFile> StoreAsync(
        Stream content,
        AttachmentFileType fileType,
        long maximumSizeBytes,
        CancellationToken cancellationToken)
    {
        var storageKey = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{fileType.Extension}";
        var targetPath = ResolveStoragePath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        HardenDirectoryPermissions(Path.GetDirectoryName(targetPath)!);
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
        long totalBytes = 0;

        try
        {
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            await using (var destination = new FileStream(
                targetPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                BufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                HardenFilePermissions(targetPath);
                while (true)
                {
                    var read = await content.ReadAsync(buffer, cancellationToken);
                    if (read == 0) break;
                    totalBytes += read;
                    if (totalBytes > maximumSizeBytes)
                        throw FileValidationException(
                            $"The attachment exceeds the maximum size of {maximumSizeBytes / 1024 / 1024} MB.");
                    hash.AppendData(buffer, 0, read);
                    await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }
                await destination.FlushAsync(cancellationToken);
            }

            if (totalBytes == 0)
                throw FileValidationException("The attachment cannot be empty.");
            if (!await AttachmentContentInspector.IsValidAsync(
                    targetPath,
                    fileType.ContentKind,
                    cancellationToken))
            {
                throw FileValidationException(
                    "The file content does not match its extension and content type.");
            }

            return new StoredAttachmentFile(
                storageKey,
                totalBytes,
                Convert.ToHexString(hash.GetHashAndReset()));
        }
        catch
        {
            File.Delete(targetPath);
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    public Task<Stream> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = new FileStream(
            ResolveStoragePath(storageKey),
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            BufferSize,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task DeleteIfExistsAsync(
        string storageKey,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        File.Delete(ResolveStoragePath(storageKey));
        return Task.CompletedTask;
    }

    private string ResolveStoragePath(string storageKey)
    {
        if (Path.IsPathRooted(storageKey))
            throw new InvalidOperationException("An attachment storage key must be relative.");
        var normalizedKey = storageKey.Replace('/', Path.DirectorySeparatorChar);
        var resolved = Path.GetFullPath(Path.Combine(_storageRoot, normalizedKey));
        var rootPrefix = _storageRoot.EndsWith(Path.DirectorySeparatorChar)
            ? _storageRoot
            : _storageRoot + Path.DirectorySeparatorChar;
        if (!resolved.StartsWith(rootPrefix, PathComparison))
            throw new InvalidOperationException("The attachment storage key is invalid.");
        return resolved;
    }

    private static ValidationException FileValidationException(string message) =>
        new([new ValidationFailure("File", message)]);

    private static bool IsAtOrBelow(string candidate, string parent)
    {
        if (candidate.Equals(parent, PathComparison)) return true;
        var parentPrefix = parent.EndsWith(Path.DirectorySeparatorChar)
            ? parent
            : parent + Path.DirectorySeparatorChar;
        return candidate.StartsWith(parentPrefix, PathComparison);
    }

    private static void HardenDirectoryPermissions(string path)
    {
        if (OperatingSystem.IsWindows()) return;
        File.SetUnixFileMode(
            path,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.UserExecute);
    }

    private static void HardenFilePermissions(string path)
    {
        if (OperatingSystem.IsWindows()) return;
        File.SetUnixFileMode(
            path,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite);
    }
}
