using FlowOps.Application;
using FlowOps.Infrastructure;
using FlowOps.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Swagger and OpenAPI generation removed for stability

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlowOpsDbContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<FlowOps.Infrastructure.Hubs.NotificationHub>("/hubs/notifications");

app.Run();
public partial class Program { }
