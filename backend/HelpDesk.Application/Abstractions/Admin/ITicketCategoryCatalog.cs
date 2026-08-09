using HelpDesk.Domain;

namespace HelpDesk.Application.Abstractions.Admin;

public interface ITicketCategoryCatalog
{
    Task EnsureActiveAsync(TicketCategory category, CancellationToken cancellationToken);
}
