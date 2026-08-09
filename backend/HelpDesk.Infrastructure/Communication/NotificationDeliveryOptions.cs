namespace HelpDesk.Infrastructure.Communication;

public sealed class NotificationDeliveryOptions
{
    public const string SectionName = "NotificationDelivery";

    public int PollIntervalSeconds { get; init; } = 5;
    public int BatchSize { get; init; } = 50;
    public int MaximumAttempts { get; init; } = 3;
}
