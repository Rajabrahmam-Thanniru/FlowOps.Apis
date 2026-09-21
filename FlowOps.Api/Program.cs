using FlowOps.Application;
using FlowOps.Infrastructure;
using FlowOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlowOpsDbContext>();
    db.Database.Migrate();
}

// Seed the database
await FlowOps.Infrastructure.Persistence.ApplicationDbInitializer.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<FlowOps.Infrastructure.Hubs.NotificationHub>("/hubs/notifications");

app.Run();
public partial class Program { }
