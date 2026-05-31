using Diplom_CRM.Data;
using Diplom_CRM.Data.Enums;
using Diplom_CRM.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Diplom_CRM.Services.Implementations;

public class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _db;

    public AnalyticsService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardMetricsDTO> GetDashboardMetricsAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Общая сумма сделок в работе
        var totalInProgress = await _db.Deals
            .Where(d => d.Status == StatusEnum.New || d.Status == StatusEnum.InProgress)
            .SumAsync(d => d.Amount);

        // Новые контакты за текущий месяц
        var newClients = await _db.Contacts
            .CountAsync(c => c.CreatedAt >= monthStart);

        // Конверсия: выигранные / закрытые (Won + Lost)
        var totalClosed = await _db.Deals
            .CountAsync(d => d.Status == StatusEnum.Won || d.Status == StatusEnum.Lost);

        double conversionRate = 0;
        if (totalClosed > 0)
        {
            var won = await _db.Deals
                .CountAsync(d => d.Status == StatusEnum.Won);
            conversionRate = Math.Round((double)won / totalClosed * 100, 1);
        }

        return new DashboardMetricsDTO
        {
            TotalDealsInProgress = totalInProgress,
            NewClientsThisMonth = newClients,
            ConversionRate = conversionRate
        };
    }

    public async Task<List<SalesFunnelItemDTO>> GetSalesFunnelDataAsync()
    {
        return await _db.Deals
            .GroupBy(d => d.Status)
            .Select(g => new SalesFunnelItemDTO
            {
                Stage = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync();
    }
}