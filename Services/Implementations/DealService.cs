using Diplom_CRM.Data;
using Diplom_CRM.Data.Entities;
using Diplom_CRM.Data.Enums;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Services;
using Microsoft.EntityFrameworkCore;
using Activity = Diplom_CRM.Data.Entities.Activity;

namespace Diplom_CRM.Services.Implementations;

public class DealService : IDealService
{
    private readonly ApplicationDbContext _db;

    public DealService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DealKanbanDTO> CreateDealAsync(DealDTO dto)
    {
        var contact = await _db.Contacts.FindAsync(dto.ContactId)
            ?? throw new KeyNotFoundException($"Контакт с Id={dto.ContactId} не найден.");

        var deal = new Deal
        {
            Name = dto.Name,
            Amount = dto.Amount,
            ExpectedCloseDate = dto.ExpectedCloseDate,
            Description = dto.Description,
            ContactId = dto.ContactId,
            Status = StatusEnum.New
        };

        _db.Deals.Add(deal);
        await _db.SaveChangesAsync();

        return new DealKanbanDTO
        {
            Id = deal.Id,
            Name = deal.Name,
            Amount = deal.Amount,
            ContactName = $"{contact.FirstName} {contact.LastName}".Trim(),
            CompanyName = contact.Company,
            ExpectedCloseDate = deal.ExpectedCloseDate
        };
    }

    public async Task ChangeDealStageAsync(int dealId, string newStage)
    {
        if (!Enum.TryParse<StatusEnum>(newStage, out var stage))
            throw new ArgumentException($"Некорректный этап сделки: {newStage}");

        var deal = await _db.Deals.FindAsync(dealId)
            ?? throw new KeyNotFoundException($"Сделка с Id={dealId} не найдена.");

        var oldStage = deal.Status;
        deal.Status = stage;

        var activity = new Activity
        {
            Type = TypeEnum.Task,
            Subject = $"Статус сделки изменён на \"{stage}\"",
            Description = $"Сделка \"{deal.Name}\": {oldStage} → {stage}",
            ScheduledDate = DateTime.UtcNow,
            IsCompleted = true,
            CompletedDate = DateTime.UtcNow,
            ContactId = deal.ContactId,
            DealId = deal.Id
        };

        _db.Activities.Add(activity);
        await _db.SaveChangesAsync();
    }

    public async Task<Dictionary<string, List<DealKanbanDTO>>> GetKanbanBoardAsync()
    {
        var deals = await _db.Deals
            .AsNoTracking()
            .Include(d => d.Contact)
            .ToListAsync();

        return deals
            .GroupBy(d => d.Status.ToString())
            .ToDictionary(
                g => g.Key,
                g => g.Select(d => new DealKanbanDTO
                {
                    Id = d.Id,
                    Name = d.Name,
                    Amount = d.Amount,
                    ContactName = $"{d.Contact.FirstName} {d.Contact.LastName}".Trim(),
                    CompanyName = d.Contact.Company,
                    ExpectedCloseDate = d.ExpectedCloseDate
                }).ToList()
            );
    }
}