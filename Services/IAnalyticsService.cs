using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;

namespace Diplom_CRM.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardMetricsDTO> GetDashboardMetricsAsync();
        Task<List<SalesFunnelItemDTO>> GetSalesFunnelDataAsync();
        Task<DashboardViewModel> GetDashboardViewModelAsync();
    }
}
