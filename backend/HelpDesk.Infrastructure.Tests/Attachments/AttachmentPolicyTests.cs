using HelpDesk.Application.Abstractions.Attachments;
using HelpDesk.Infrastructure.Attachments;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Tests.Attachments;

public sealed class AttachmentPolicyTests
{
    private readonly AttachmentPolicy _policy = new(Options.Create(new AttachmentOptions()));

    [TestCase("screen.png", "image/png", AttachmentContentKind.Png)]
    [TestCase("error.LOG", "application/octet-stream", AttachmentContentKind.PlainText)]
    [TestCase("report.pdf", "application/pdf; charset=binary", AttachmentContentKind.Pdf)]
    [TestCase("trace.zip", "application/x-zip-compressed", AttachmentContentKind.Zip)]
    [TestCase("document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", AttachmentContentKind.WordOpenXml)]
    public void Match_SupportedMetadata_ReturnsCanonicalFileType(
        string fileName,
        string contentType,
        AttachmentContentKind expectedKind)
    {
        var result = _policy.Match(fileName, contentType);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ContentKind, Is.EqualTo(expectedKind));
    }

    [TestCase("script.exe", "application/octet-stream")]
    [TestCase("fake.png", "application/pdf")]
    [TestCase("archive.zip", "text/plain")]
    public void Match_UnsupportedOrMismatchedMetadata_ReturnsNull(
        string fileName,
        string contentType) =>
        Assert.That(_policy.Match(fileName, contentType), Is.Null);
}
