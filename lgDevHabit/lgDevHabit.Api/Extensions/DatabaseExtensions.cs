using lgDevHabit.Api.Database;
using lgDevHabit.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace lgDevHabit.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        await using ApplicationDbContext applicationDbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await using ApplicationIdentityDbContext applicationIdentityDbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        try
        {
            //Application migration 
            await applicationDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Database migrations applied successfully.");

            //Identity migration
            await applicationIdentityDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Identity database migrations applied successfully.");
        }
        catch (Exception e)
        {
            app.Logger.LogError(e, "An error occurred while applying database migrations.");
            throw;
        }
    }


    public static async Task SeedInitialDataAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        RoleManager<IdentityRole> roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        try
        {
            if (!await roleManager.RoleExistsAsync(Roles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            }
            if (!await roleManager.RoleExistsAsync(Roles.Member))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Member));
            }

            app.Logger.LogInformation("Successfuly created roles.");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while seeding initial data.");
            throw;
        }
    }
}
