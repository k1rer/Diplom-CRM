using Microsoft.AspNetCore.Identity;

namespace Diplom_CRM.Data;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            string[] roles = ["Admin", "Manager", "User"];

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Системная роль '{RoleName}' успешно создана.", roleName);
                    }
                    else
                    {
                        logger.LogError("Ошибка при создании роли '{RoleName}': {Errors}",
                            roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Произошла ошибка при первичной инициализации ролей.");
            throw;
        }
    }
}