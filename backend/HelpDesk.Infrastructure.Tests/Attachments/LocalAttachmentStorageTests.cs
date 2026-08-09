using System.Text;
using FluentValidation;
using HelpDesk.Application.Abstractions.Attachments;
using HelpDesk.Infrastructure.Attachments;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Tests.Attachments;

public sealed class LocalAttachmentStorageTests
{
    private string _testRoot = null!;
    private LocalAttachmentStorage _storage = null!;

    [SetUp]
    public void SetUp()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), $"helpdesk-attachments-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testRoot);
        _storage = new LocalAttachmentStorage(
            new TestHostEnvironment(_testRoot),
            Options.Create(new AttachmentOptions { StorageRootPath = "files" }));
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testRoot))
            Directory.Delete(_testRoot, recursive: true);
    }

    [Test]
    public async Task StoreAsync_ValidPng_StoresByGeneratedKeyAndHashesContent()
    {
        byte[] content = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3];
        await using var source = new MemoryStream(content);

        var stored = await _storage.StoreAsync(
            source,
            new AttachmentFileType(".png", "image/png", AttachmentContentKind.Png),
            1024,
            CancellationToken.None);
        await using var downloaded = await _storage.OpenReadAsync(
            stored.StorageKey,
            CancellationToken.None);
        using var copy = new MemoryStream();
        await downloaded.CopyToAsync(copy);

        Assert.Multiple(() =>
        {
            Assert.That(stored.StorageKey, Does.EndWith(".png"));
            Assert.That(stored.StorageKey, Does.Not.Contain(".."));
            Assert.That(stored.SizeBytes, Is.EqualTo(content.Length));
            Assert.That(stored.Sha256Hash, Has.Length.EqualTo(64));
            Assert.That(copy.ToArray(), Is.EqualTo(content));
        });
    }

    [Test]
    public void StoreAsync_DisguisedFile_RejectsAndRemovesStagedContent()
    {
        var source = new MemoryStream(Encoding.UTF8.GetBytes("not a png"));

        Assert.ThrowsAsync<ValidationException>(() => _storage.StoreAsync(
            source,
            new AttachmentFileType(".png", "image/png", AttachmentContentKind.Png),
            1024,
            CancellationToken.None));
        Assert.That(
            Directory.EnumerateFiles(_testRoot, "*", SearchOption.AllDirectories),
            Is.Empty);
    }

    [Test]
    public void StoreAsync_ActualContentExceedsLimit_RejectsUpload()
    {
        var source = new MemoryStream(new byte[32]);

        Assert.ThrowsAsync<ValidationException>(() => _storage.StoreAsync(
            source,
            new AttachmentFileType(".txt", "text/plain", AttachmentContentKind.PlainText),
            16,
            CancellationToken.None));
    }

    [Test]
    public void OpenReadAsync_PathTraversalKey_IsRejected() =>
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _storage.OpenReadAsync("../outside.txt", CancellationToken.None));

    [Test]
    public void Constructor_PublicWebRootStorage_IsRejected() =>
        Assert.Throws<InvalidOperationException>(() => new LocalAttachmentStorage(
            new TestHostEnvironment(_testRoot),
            Options.Create(new AttachmentOptions
            {
                StorageRootPath = "wwwroot/attachments"
            })));

    private sealed class TestHostEnvironment(string contentRoot) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "HelpDesk.Infrastructure.Tests";
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
