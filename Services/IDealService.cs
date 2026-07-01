using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Diplom_CRM.Services
{
    public interface IDealService
    {
        Task<DealKanbanDTO> CreateDealAsync(DealDTO dto);
        Task ChangeDealStageAsync(int dealId, string newStage);
        Task<Dictionary<string, List<DealKanbanDTO>>> GetKanbanBoardAsync();
        Task<DealKanbanDTO> GetDealKanbanByIdAsync(int dealId);
        Task<List<SelectListItem>> GetDealsSelectListAsync();
        Task<DealDetailsViewModel?> GetDealDetailsAsync(int dealId);
    }
}
