using Diplom_CRM.Models.DTO;

namespace Diplom_CRM.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardMetricsDTO> GetDashboardMetricsAsync();
        Task<List<SalesFunnelItemDTO>> GetSalesFunnelDataAsync();
    }
}
