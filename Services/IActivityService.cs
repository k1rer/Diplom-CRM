using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;

namespace Diplom_CRM.Services
{
    public interface IActivityService
    {
        Task<ActivityListItemDTO> CreateActivityAsync(ActivityDTO dto);
        Task ToggleTaskCompletionAsync(int activityId);
        Task<List<ActivityListItemDTO>> GetPendingTasksForUserAsync(string userId);
        Task<List<ActivityDTO>> GetActivitiesByCompanyIdAsync(int companyId);
        Task DeleteActivityAsync(int activityId);
        Task<List<ActivityDTO>> GetFilteredActivitiesAsync(ActivityFilterViewModel filter);
        Task<ActivityDTO?> GetActivityByIdAsync(int activityId);
    }
}
