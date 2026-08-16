using Diplom_CRM.Models;

namespace Diplom_CRM.Services
{
    public interface IUserService
    {
        Task<bool> AnyUsersExistAsync();
        Task AssignAdminRoleAsync(AppUser user);
    }
}
