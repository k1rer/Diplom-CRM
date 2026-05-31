using Diplom_CRM.Models.DTO;

namespace Diplom_CRM.Models.View
{
    public class DashboardViewModel
    {
        public DashboardMetricsDTO Metrics { get; set; } = new();
        public List<SalesFunnelItemDTO> Funnel { get; set; } = new();
    }
}
