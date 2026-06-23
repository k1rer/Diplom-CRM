using Diplom_CRM.Models.DTO;

namespace Diplom_CRM.Models.View
{
    public class DashboardViewModel
    {
        public int ActiveClientsCount { get; set; }
        public decimal TotalPipelineAmount { get; set; }
        public double WinRate { get; set; }
        public int PendingTasksCount { get; set; }
        public Dictionary<string, int> StageCounts { get; set; } = new();
        public Dictionary<string, decimal> StageAmounts { get; set; } = new();
        public List<DealSummaryDTO> TopDeals { get; set; } = new();
    }
}
