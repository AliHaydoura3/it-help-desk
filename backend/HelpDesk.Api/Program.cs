using HelpDesk.Infrastructure;
using HelpDesk.Infrastructure.Identity.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapControllers();

using var scope = app.Services.CreateScope();
await scope.ServiceProvider.GetRequiredService<RoleSeeder>().SeedAsync();
await scope.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync();

app.UseHttpsRedirection();

app.Run();
