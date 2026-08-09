using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HelpDesk.Api.Notifications;

[Authorize]
public sealed class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub")
            ?? throw new HubException("The authenticated user identifier is missing.");

        await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroup(userId));
        await base.OnConnectedAsync();
    }

    internal static string GetUserGroup(string userId) => $"notifications:user:{userId}";
}
