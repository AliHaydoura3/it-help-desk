using System.Security.Claims;
using HelpDesk.Domain;
using HelpDesk.Infrastructure.Persistence;

namespace HelpDesk.Api.Middleware;

public sealed class ActivityLoggingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ApplicationDbContext dbContext)
    {
        await next(context);

        if (!context.Request.Path.StartsWithSegments("/api"))
            return;

        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub");
        var email = context.User.FindFirstValue(ClaimTypes.Email)
            ?? context.User.FindFirstValue("email");

        dbContext.UserActivityLogs.Add(new UserActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = Guid.TryParse(userIdValue, out var userId) ? userId : null,
            UserEmail = email,
            Action = context.Request.Method,
            Resource = context.Request.Path.Value ?? string.Empty,
            ResourceId = context.Request.RouteValues.TryGetValue("id", out var id)
                ? id?.ToString()
                : null,
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            Succeeded = context.Response.StatusCode < 400,
            OccurredAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(context.RequestAborted);
    }
}
