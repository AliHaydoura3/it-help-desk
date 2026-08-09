namespace HelpDesk.Application.Abstractions.Communication;

public interface INotificationDeliveryProcessor
{
    Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default);
}
