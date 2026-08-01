using HelpDesk.Api.Exceptions;
using HelpDesk.Application;
using HelpDesk.Application.Common.Authorization;
using HelpDesk.Infrastructure;
using HelpDesk.Infrastructure.Identity.Seeding;
using HelpDesk.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


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
            .AllowAnyMethod();
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

app.MapGet("/admin", () =>
{
    return Results.Ok(new
    {
        Message = "Welcome, Admin!"
    });
})
.RequireAuthorization(Policies.AdminOnly);

app.Run();
