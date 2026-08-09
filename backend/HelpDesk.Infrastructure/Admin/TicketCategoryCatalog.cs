using HelpDesk.Application.Abstractions.Admin;
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Admin;

public sealed class TicketCategoryCatalog(ApplicationDbContext dbContext)
    : ITicketCategoryCatalog
{
    public async Task EnsureActiveAsync(TicketCategory category, CancellationToken cancellationToken)
    {
        var isActive = await dbContext.TicketCategorySettings.AsNoTracking()
            .AnyAsync(setting => setting.Category == category && setting.IsActive, cancellationToken);

        if (!isActive)
            throw new InvalidOperationException($"The {category} ticket category is inactive.");
    }
}
