using Diplom_CRM.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Diplom_CRM.Services.Implementations
{
    public class UserService(
        UserManager<AppUser> userManager,
        ILogger<UserService> logger) : IUserService
    {
        public async Task<bool> AnyUsersExistAsync()
        {
            return await userManager.Users.AnyAsync();
        }

        public async Task AssignAdminRoleAsync(AppUser user)
        {
            var result = await userManager.AddToRoleAsync(user, "Admin");

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError("Не удалось назначить роль Admin пользователю {UserId}: {Errors}", user.Id, errors);
                throw new InvalidOperationException($"Ошибка назначения роли Admin: {errors}");
            }
        }
    }
}
