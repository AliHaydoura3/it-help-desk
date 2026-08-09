using System.Text;
using ClosedXML.Excel;
using HelpDesk.Application.Features.Reporting.Exports;
using HelpDesk.Infrastructure.Reporting;
using HelpDesk.Infrastructure.Reporting.Exports;
using Microsoft.Extensions.Options;

namespace HelpDesk.Infrastructure.Tests.Reporting;

public sealed class ReportExporterTests
{
    private static readonly ReportDocument Document = new(
        "Export smoke test",
        "2026-08-01 to 2026-08-07",
        new DateTime(2026, 8, 7, 12, 0, 0, DateTimeKind.Utc),
        [new ReportSection(
            "Ticket metrics",
            ["Metric", "Value"],
            [["Open", "4"], ["Resolved", "7"]])]);

    [Test]
    public void ExcelExporter_ProducesReadableWorkbook()
    {
        var result = new ExcelReportExporter().Export(Document);

        using var stream = new MemoryStream(result.Content);
        using var workbook = new XLWorkbook(stream);
        Assert.Multiple(() =>
        {
            Assert.That(result.ContentType, Is.EqualTo(
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            Assert.That(result.FileName, Does.EndWith(".xlsx"));
            Assert.That(workbook.Worksheets.Count, Is.EqualTo(1));
            Assert.That(workbook.Worksheet(1).Cell(6, 1).GetString(), Is.EqualTo("Open"));
        });
    }

    [Test]
    public void PdfExporter_ProducesPdfDocument()
    {
        const string regular = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";
        const string bold = "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf";
        if (!File.Exists(regular) || !File.Exists(bold))
            Assert.Ignore("DejaVu fonts are not installed in this test environment.");
        var exporter = new PdfReportExporter(Options.Create(new ReportingOptions
        {
            PdfRegularFontPath = regular,
            PdfBoldFontPath = bold
        }));

        var result = exporter.Export(Document);

        Assert.Multiple(() =>
        {
            Assert.That(result.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(result.FileName, Does.EndWith(".pdf"));
            Assert.That(Encoding.ASCII.GetString(result.Content, 0, 5), Is.EqualTo("%PDF-"));
            Assert.That(result.Content.Length, Is.GreaterThan(500));
        });
    }
}
