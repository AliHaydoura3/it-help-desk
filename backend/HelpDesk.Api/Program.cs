using System.Text.Json.Serialization;
using HelpDesk.Api.Exceptions;
using HelpDesk.Application;
using HelpDesk.Infrastructure;
using HelpDesk.Infrastructure.Identity.Seeding;
using HelpDesk.Api.Middleware;
using HelpDesk.Api.Notifications;
using HelpDesk.Application.Abstractions.Communication;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

var configuredAttachmentLimit = builder.Configuration.GetValue<long?>(
    "Attachments:MaximumFileSizeBytes") ?? 10 * 1024 * 1024;
var attachmentFormLimit = Math.Clamp(
    configuredAttachmentLimit,
    1024,
    25 * 1024 * 1024) + 1024 * 1024;
builder.Services.Configure<FormOptions>(options =>
    options.MultipartBodyLengthLimit = attachmentFormLimit);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSignalR();
builder.Services.AddSingleton<INotificationRealtimePublisher,
    SignalRNotificationRealtimePublisher>();
builder.Services.AddHostedService<NotificationDeliveryWorker>();


builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider
        .GetRequiredService<IdentitySeeder>();

    await seeder.SeedAsync();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.UseMiddleware<ActivityLoggingMiddleware>();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
