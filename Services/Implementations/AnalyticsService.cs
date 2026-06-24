using Diplom_CRM.Data;
using Diplom_CRM.Data.Enums;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;
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

    public async Task<DashboardViewModel> GetDashboardViewModelAsync()
    {
        var now = DateTime.UtcNow;

        // Активные клиенты
        var activeClients = await _db.Companies.CountAsync();

        // Объем воронки (New + InProgress)
        var pipelineAmount = await _db.Deals
            .Where(d => d.Status == StatusEnum.New || d.Status == StatusEnum.InProgress)
            .SumAsync(d => d.Amount);

        // Win Rate
        var totalWon = await _db.Deals.CountAsync(d => d.Status == StatusEnum.Won);
        var totalLost = await _db.Deals.CountAsync(d => d.Status == StatusEnum.Lost);
        double winRate = 0;
        if (totalWon + totalLost > 0)
            winRate = Math.Round((double)totalWon / (totalWon + totalLost) * 100, 1);

        // Задачи в работе
        var pendingTasks = await _db.Activities
            .CountAsync(a => !a.IsCompleted && a.Type == TypeEnum.Task);

        // Количество сделок по стадиям
        var stageCounts = await _db.Deals
            .GroupBy(d => d.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync();

        var countsDict = stageCounts.ToDictionary(x => x.Key.ToString(), x => x.Count);

        // Суммы по стадиям
        var stageAmounts = await _db.Deals
            .GroupBy(d => d.Status)
            .Select(g => new { g.Key, Total = g.Sum(d => d.Amount) })
            .ToListAsync();

        var amountsDict = stageAmounts.ToDictionary(x => x.Key.ToString(), x => x.Total);

        // Топ-5 сделок
        var topDeals = await _db.Deals
            .Where(d => d.Status == StatusEnum.New || d.Status == StatusEnum.InProgress)
            .OrderByDescending(d => d.Amount)
            .Take(5)
            .Include(d => d.Contact)
            .Select(d => new DealSummaryDTO
            {
                Id = d.Id,
                Name = d.Name,
                Amount = d.Amount,
                CompanyName = d.Contact.Company ?? "—"
            })
            .ToListAsync();

        return new DashboardViewModel
        {
            ActiveClientsCount = activeClients,
            TotalPipelineAmount = pipelineAmount,
            WinRate = winRate,
            PendingTasksCount = pendingTasks,
            StageCounts = countsDict,
            StageAmounts = amountsDict,
            TopDeals = topDeals
        };
    }
}