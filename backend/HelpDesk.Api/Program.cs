using HelpDesk.Infrastructure;
using HelpDesk.Infrastructure.Identity.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using var scope = app.Services.CreateScope();
await scope.ServiceProvider.GetRequiredService<RoleSeeder>().SeedAsync();
await scope.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync();

app.UseHttpsRedirection();

app.Run();
