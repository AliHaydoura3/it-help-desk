using HelpDesk.Domain;

namespace HelpDesk.Infrastructure.Reporting;

public sealed class ReportingOptions
{
    public const string SectionName = "Reporting";

    public int DefaultPeriodDays { get; init; } = 30;
    public double AtRiskThresholdPercentage { get; init; } = 80;
    public string PdfRegularFontPath { get; init; } =
        "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";
    public string PdfBoldFontPath { get; init; } =
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf";
    public SlaTargetOptions SlaHours { get; init; } = new();

    public double GetSlaHours(TicketPriority priority) => priority switch
    {
        TicketPriority.Low => SlaHours.Low,
        TicketPriority.Medium => SlaHours.Medium,
        TicketPriority.High => SlaHours.High,
        TicketPriority.Critical => SlaHours.Critical,
        _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, null)
    };
}

public sealed class SlaTargetOptions
{
    public double Low { get; init; } = 72;
    public double Medium { get; init; } = 48;
    public double High { get; init; } = 24;
    public double Critical { get; init; } = 4;
}
