using HelpDesk.Api.Exceptions;
using HelpDesk.Application;
using HelpDesk.Application.Authorization;
using HelpDesk.Infrastructure;
using HelpDesk.Infrastructure.Identity.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider
        .GetRequiredService<IdentitySeeder>();

    await seeder.SeedAsync();
}

app.UseHttpsRedirection();
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