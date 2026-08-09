using ClosedXML.Excel;
using HelpDesk.Application.Abstractions.Reporting;
using HelpDesk.Application.Features.Reporting.Exports;

namespace HelpDesk.Infrastructure.Reporting.Exports;

public sealed class ExcelReportExporter : IReportFileExporter
{
    public ReportExportFormat Format => ReportExportFormat.Excel;

    public ReportFileResponse Export(ReportDocument document)
    {
        using var workbook = new XLWorkbook();
        workbook.Properties.Title = document.Title;
        workbook.Properties.Subject = document.Subtitle;
        workbook.Properties.Author = "IT Help Desk";

        foreach (var section in document.Sections)
        {
            var worksheet = workbook.Worksheets.Add(GetUniqueWorksheetName(workbook, section.Title));
            worksheet.Cell(1, 1).Value = document.Title;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Range(1, 1, 1, Math.Max(1, section.Columns.Count)).Merge();
            worksheet.Cell(2, 1).Value = document.Subtitle;
            worksheet.Range(2, 1, 2, Math.Max(1, section.Columns.Count)).Merge();
            worksheet.Cell(3, 1).Value = $"Generated {document.GeneratedAtUtc:u}";
            worksheet.Range(3, 1, 3, Math.Max(1, section.Columns.Count)).Merge();

            const int headerRow = 5;
            for (var column = 0; column < section.Columns.Count; column++)
                worksheet.Cell(headerRow, column + 1).Value = section.Columns[column];
            var header = worksheet.Range(headerRow, 1, headerRow, section.Columns.Count);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#E5E7EB");
            header.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            for (var row = 0; row < section.Rows.Count; row++)
            {
                for (var column = 0; column < section.Columns.Count; column++)
                {
                    worksheet.Cell(headerRow + row + 1, column + 1).Value =
                        column < section.Rows[row].Count ? section.Rows[row][column] : string.Empty;
                }
            }

            worksheet.SheetView.FreezeRows(headerRow);
            worksheet.Columns().AdjustToContents(1, 60);
            if (section.Rows.Count > 0)
            {
                worksheet.Range(
                    headerRow,
                    1,
                    headerRow + section.Rows.Count,
                    section.Columns.Count).SetAutoFilter();
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new ReportFileResponse(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            BuildFileName(document.Title, "xlsx"));
    }

    private static string GetUniqueWorksheetName(XLWorkbook workbook, string title)
    {
        var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
        var baseName = new string(title.Where(character => !invalid.Contains(character)).ToArray());
        baseName = string.IsNullOrWhiteSpace(baseName) ? "Report" : baseName[..Math.Min(31, baseName.Length)];
        var name = baseName;
        var suffix = 2;
        while (workbook.Worksheets.Any(sheet => sheet.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            var suffixText = $" {suffix++}";
            name = baseName[..Math.Min(31 - suffixText.Length, baseName.Length)] + suffixText;
        }
        return name;
    }

    private static string BuildFileName(string title, string extension) =>
        $"{string.Join('-', title.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries))}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{extension}";
}
