using HelpDesk.Application.Abstractions.Reporting;
using HelpDesk.Application.Features.Reporting.Exports;
using Microsoft.Extensions.Options;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace HelpDesk.Infrastructure.Reporting.Exports;

public sealed class PdfReportExporter : IReportFileExporter
{
    private const string FontFamily = "HelpDeskSans";
    private static readonly object FontLock = new();
    private readonly ReportingOptions _options;

    public PdfReportExporter(IOptions<ReportingOptions> options)
    {
        _options = options.Value;
        EnsureFontResolver();
    }

    public ReportExportFormat Format => ReportExportFormat.Pdf;

    public ReportFileResponse Export(ReportDocument report)
    {
        using var document = new PdfDocument();
        document.Info.Title = report.Title;
        document.Info.Subject = report.Subtitle;
        document.Info.Author = "IT Help Desk";
        var renderer = new PdfTableRenderer(document, report);
        renderer.Render();

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return new ReportFileResponse(
            stream.ToArray(),
            "application/pdf",
            BuildFileName(report.Title, "pdf"));
    }

    private void EnsureFontResolver()
    {
        if (GlobalFontSettings.FontResolver is not null) return;
        lock (FontLock)
        {
            GlobalFontSettings.FontResolver ??= new PdfFileFontResolver(
                _options.PdfRegularFontPath,
                _options.PdfBoldFontPath);
        }
    }

    private static string BuildFileName(string title, string extension) =>
        $"{string.Join('-', title.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries))}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{extension}";

    private sealed class PdfTableRenderer(PdfDocument document, ReportDocument report)
    {
        private const double Margin = 36;
        private const double RowHeight = 22;
        private readonly XFont _titleFont = new(FontFamily, 18, XFontStyleEx.Bold);
        private readonly XFont _subtitleFont = new(FontFamily, 9, XFontStyleEx.Regular);
        private readonly XFont _sectionFont = new(FontFamily, 13, XFontStyleEx.Bold);
        private readonly XFont _headerFont = new(FontFamily, 8, XFontStyleEx.Bold);
        private readonly XFont _cellFont = new(FontFamily, 8, XFontStyleEx.Regular);
        private PdfPage _page = null!;
        private XGraphics _graphics = null!;
        private double _y;

        public void Render()
        {
            AddPage(includeReportHeader: true);
            foreach (var section in report.Sections)
            {
                EnsureSpace(56, includeTableHeader: false, section: null);
                _graphics.DrawString(section.Title, _sectionFont, XBrushes.Black,
                    new XRect(Margin, _y, ContentWidth, 22), XStringFormats.TopLeft);
                _y += 26;
                DrawHeader(section);
                if (section.Rows.Count == 0)
                {
                    DrawEmptyRow(section.Columns.Count);
                    continue;
                }

                foreach (var row in section.Rows)
                {
                    EnsureSpace(RowHeight, includeTableHeader: true, section);
                    DrawRow(section.Columns.Count, row);
                }
                _y += 18;
            }
            _graphics.Dispose();
        }

        private double ContentWidth => _page.Width.Point - Margin * 2;

        private void AddPage(bool includeReportHeader)
        {
            _graphics?.Dispose();
            _page = document.AddPage();
            _page.Orientation = PdfSharp.PageOrientation.Landscape;
            _graphics = XGraphics.FromPdfPage(_page);
            _y = Margin;
            if (!includeReportHeader) return;

            _graphics.DrawString(report.Title, _titleFont, XBrushes.Black,
                new XRect(Margin, _y, ContentWidth, 26), XStringFormats.TopLeft);
            _y += 28;
            _graphics.DrawString(report.Subtitle, _subtitleFont, XBrushes.DimGray,
                new XRect(Margin, _y, ContentWidth, 16), XStringFormats.TopLeft);
            _y += 15;
            _graphics.DrawString($"Generated {report.GeneratedAtUtc:u}", _subtitleFont, XBrushes.DimGray,
                new XRect(Margin, _y, ContentWidth, 16), XStringFormats.TopLeft);
            _y += 28;
        }

        private void EnsureSpace(
            double requiredHeight,
            bool includeTableHeader,
            ReportSection? section)
        {
            if (_y + requiredHeight <= _page.Height.Point - Margin) return;
            AddPage(includeReportHeader: false);
            if (includeTableHeader && section is not null) DrawHeader(section);
        }

        private void DrawHeader(ReportSection section)
        {
            DrawCells(section.Columns.Count, section.Columns, _headerFont, XBrushes.White, XBrushes.DarkSlateGray);
        }

        private void DrawRow(int columnCount, IReadOnlyList<string> values)
        {
            DrawCells(columnCount, values, _cellFont, XBrushes.Black, XBrushes.WhiteSmoke);
        }

        private void DrawEmptyRow(int columnCount)
        {
            DrawCells(columnCount, ["No data"], _cellFont, XBrushes.DimGray, XBrushes.WhiteSmoke);
        }

        private void DrawCells(
            int columnCount,
            IReadOnlyList<string> values,
            XFont font,
            XBrush foreground,
            XBrush background)
        {
            var width = ContentWidth / Math.Max(1, columnCount);
            for (var column = 0; column < columnCount; column++)
            {
                var rectangle = new XRect(Margin + column * width, _y, width, RowHeight);
                _graphics.DrawRectangle(XPens.LightGray, background, rectangle);
                var value = column < values.Count ? values[column] : string.Empty;
                _graphics.DrawString(Truncate(value, width), font, foreground,
                    new XRect(rectangle.X + 4, rectangle.Y + 5, rectangle.Width - 8, rectangle.Height - 6),
                    XStringFormats.TopLeft);
            }
            _y += RowHeight;
        }

        private static string Truncate(string value, double width)
        {
            var maxCharacters = Math.Max(4, (int)(width / 4.7));
            return value.Length <= maxCharacters
                ? value
                : value[..Math.Max(1, maxCharacters - 1)] + "…";
        }
    }
}
