using Diplom_CRM.Data;
using Diplom_CRM.Models;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Diplom_CRM.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public AdminService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public async Task<List<UserDTO>> GetUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserDTO>();

            foreach (var user in users)
            {
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Manager";
                var dealsCount = await _db.Deals
                    .CountAsync(d => d.AppUserId == user.Id &&
                                     (d.Status == Data.Enums.StatusEnum.New || d.Status == Data.Enums.StatusEnum.InProgress));

                result.Add(new UserDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? "",
                    Role = role,
                    DealsInProgressCount = dealsCount,
                    IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow
                });
            }

            return result;
        }

        public async Task<UserDTO?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Manager";
            var dealsCount = await _db.Deals
                .CountAsync(d => d.AppUserId == user.Id &&
                                 (d.Status == Data.Enums.StatusEnum.New || d.Status == Data.Enums.StatusEnum.InProgress));

            return new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? "",
                Role = role,
                DealsInProgressCount = dealsCount,
                IsLocked = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow
            };
        }

        public async Task ChangeRoleAsync(string userId, string newRole)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("Пользователь не найден.");
            var currentRoles = await _userManager.GetRolesAsync(user);

            if (!await _roleManager.RoleExistsAsync(newRole))
                await _roleManager.CreateAsync(new IdentityRole(newRole));

            // Удаляем текущие роли и добавляем новую
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);
        }

        public async Task ToggleLockoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException("Пользователь не найден.");

            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                // Разблокировать
                user.LockoutEnd = null;
            }
            else
            {
                // Заблокировать навсегда (или на длительный срок)
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }

            await _userManager.UpdateAsync(user);
        }

        public async Task CreateUserAsync(RegisterViewModel model, string role)
        {
            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            await _userManager.AddToRoleAsync(user, role);
        }
    }
}