using System;
using System.Linq;
using System.Threading.Tasks;
using FlowOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FlowOps.Infrastructure.Persistence;

public static class ApplicationDbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FlowOpsDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("ApplicationDbInitializer");

        try
        {
            if (context.Database.IsNpgsql())
            {
                await context.Database.MigrateAsync();
            }

            // Check if SuperAdmin exists
            if (!await context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == "raja9392t@gmail.com"))
            {
                logger.LogInformation("Seeding Super Admin user.");

                var superAdminOrg = await context.Organizations.FirstOrDefaultAsync(o => o.Name == "Super Admin Org");
                if (superAdminOrg == null)
                {
                    superAdminOrg = new Organization
                    {
                        Name = "Super Admin Org",
                        Slug = "super-admin-org"
                    };
                    context.Organizations.Add(superAdminOrg);
                    await context.SaveChangesAsync();
                }

                var superAdminRole = await context.Roles.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
                if (superAdminRole == null)
                {
                    superAdminRole = new Role
                    {
                        Name = "SuperAdmin",
                        Description = "Super Administrator with system-wide access",
                        TenantId = superAdminOrg.Id
                    };
                    context.Roles.Add(superAdminRole);
                    await context.SaveChangesAsync();
                }

                var superAdminUser = new User
                {
                    Email = "raja9392t@gmail.com",
                    FirstName = "Super",
                    LastName = "Admin",
                    PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword("R2a8j2a8@"),
                    TenantId = superAdminOrg.Id,
                    Organization = superAdminOrg
                };
                
                superAdminUser.UserRoles.Add(new UserRole { User = superAdminUser, Role = superAdminRole });
                context.Users.Add(superAdminUser);

                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
