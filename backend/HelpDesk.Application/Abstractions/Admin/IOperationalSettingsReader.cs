namespace HelpDesk.Application.Abstractions.Admin;

public sealed record OperationalSettingsSnapshot(
    bool AutomaticAssignmentEnabled,
    bool EmailNotificationsEnabled,
    int MaximumOpenTicketsPerEmployee);

public interface IOperationalSettingsReader
{
    Task<OperationalSettingsSnapshot> GetAsync(CancellationToken cancellationToken);
}
