using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;

namespace Diplom_CRM.Services
{
    public interface IAdminService
    {
        Task<List<UserDTO>> GetUsersAsync();
        Task<UserDTO?> GetUserByIdAsync(string userId);
        Task ChangeRoleAsync(string userId, string newRole);
        Task ToggleLockoutAsync(string userId);
        Task CreateUserAsync(RegisterViewModel model, string role);
    }
}
