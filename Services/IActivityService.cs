using Diplom_CRM.Models.DTO;

namespace Diplom_CRM.Services
{
    public interface IActivityService
    {
        Task<ActivityListItemDTO> CreateActivityAsync(ActivityDTO dto);
        Task SetTaskAsCompletedAsync(int activityId);
        Task<List<ActivityListItemDTO>> GetPendingTasksForUserAsync(string userId);
    }
}
