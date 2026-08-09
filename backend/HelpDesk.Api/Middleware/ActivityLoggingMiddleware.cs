using System.Security.Claims;
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Persistence;

namespace HelpDesk.Api.Middleware;

public sealed class ActivityLoggingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IServiceScopeFactory scopeFactory)
    {
        await next(context);

        if (!context.Request.Path.StartsWithSegments("/api"))
            return;

        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");
        var email = context.User.FindFirstValue(ClaimTypes.Email)
            ?? context.User.FindFirstValue("email");

        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var resource = context.Request.Path.Value ?? string.Empty;
        dbContext.UserActivityLogs.Add(new UserActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.TryParse(userIdValue, out var userId) ? userId : null,
            UserEmail = email,
            Action = context.Request.Method,
            Resource = Truncate(resource, 100),
            ResourceId = GetResourceId(context),
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            Succeeded = context.Response.StatusCode < 400,
            OccurredAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(CancellationToken.None);
    }

    private static string? GetResourceId(HttpContext context)
    {
        foreach (var key in new[] { "attachmentId", "ticketId", "notificationId", "id" })
        {
            if (context.Request.RouteValues.TryGetValue(key, out var value) && value is not null)
                return Truncate(value.ToString()!, 100);
        }
        return null;
    }

    private static string Truncate(string value, int maximumLength) =>
        value.Length <= maximumLength ? value : value[..maximumLength];
}
