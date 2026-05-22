using Diplom_CRM.Models.DTO;

namespace Diplom_CRM.Services
{
    public interface IDealService
    {
        Task<DealKanbanDTO> CreateDealAsync(DealDTO dto);
        Task ChangeDealStageAsync(int dealId, string newStage);
        Task<Dictionary<string, List<DealKanbanDTO>>> GetKanbanBoardAsync();
    }
}
