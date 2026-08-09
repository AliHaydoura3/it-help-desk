using HelpDesk.Application.Abstractions.Admin;
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Admin;

public sealed class OperationalSettingsReader(ApplicationDbContext dbContext)
    : IOperationalSettingsReader
{
    public async Task<OperationalSettingsSnapshot> GetAsync(CancellationToken cancellationToken)
    {
        var settings = await dbContext.SystemSettings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == SystemSettings.SingletonId, cancellationToken);

        settings ??= SystemSettings.CreateDefaults(DateTime.UtcNow);
        return new OperationalSettingsSnapshot(
            settings.AutomaticAssignmentEnabled,
            settings.EmailNotificationsEnabled,
            settings.MaximumOpenTicketsPerEmployee);
    }
}
