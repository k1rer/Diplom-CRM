using Diplom_CRM.Data;
using Diplom_CRM.Data.Entities;
using Diplom_CRM.Data.Enums;
using Diplom_CRM.Extensions;
using Diplom_CRM.Models;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Activity = Diplom_CRM.Data.Entities.Activity;

namespace Diplom_CRM.Services.Implementations;

public class DealService : IDealService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<AppUser> _userManager;

    public DealService(
        ApplicationDbContext db,
        IHttpContextAccessor httpContextAccessor,
        UserManager<AppUser> userManager)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<DealKanbanDTO> CreateDealAsync(DealDTO dto)
    {
        var contact = await _db.Contacts.FindAsync(dto.ContactId)
            ?? throw new KeyNotFoundException($"Контакт с Id={dto.ContactId} не найден.");

        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext отсутствует.");

        var principal = httpContext.User
            ?? throw new InvalidOperationException("Пользователь не аутентифицирован.");

        var currentUser = await _userManager.GetUserAsync(principal)
            ?? throw new InvalidOperationException("Пользователь не найден в базе.");

        if (dto.ExpectedCloseDate.Kind == DateTimeKind.Unspecified)
            dto.ExpectedCloseDate = DateTime.SpecifyKind(dto.ExpectedCloseDate, DateTimeKind.Local).ToUniversalTime();
        else if (dto.ExpectedCloseDate.Kind == DateTimeKind.Local)
            dto.ExpectedCloseDate = dto.ExpectedCloseDate.ToUniversalTime();

        var deal = new Deal
        {
            Name = dto.Name,
            Amount = dto.Amount,
            ExpectedCloseDate = dto.ExpectedCloseDate,
            Status = StatusEnum.New,
            ContactId = dto.ContactId,
            AppUserId = currentUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Deals.Add(deal);
        await _db.SaveChangesAsync();

        var activity = new Activity
        {
            Type = TypeEnum.Task,
            Subject = $"Создана сделка \"{deal.Name}\"",
            Description = $"Сумма: {deal.Amount:C}, Статус: New",
            ScheduledDate = DateTime.UtcNow,
            IsCompleted = false,
            CompletedDate = null, 
            ContactId = deal.ContactId,
            DealId = deal.Id
        };
        _db.Activities.Add(activity);
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

        _db.Entry(deal).State = EntityState.Modified;

        var activity = new Activity
        {
            Type = TypeEnum.Task,
            Subject = $"Статус сделки изменён на \"{stage.GetDisplayName()}\"",
            Description = $"Сделка \"{deal.Name}\": {oldStage.GetDisplayName()} → {stage.GetDisplayName()}",
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
            .Include(d => d.Activities)
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
                    ExpectedCloseDate = d.ExpectedCloseDate,
                    ActivitiesCount = d.Activities.Count
                }).ToList()
            );
    }

    public async Task<DealKanbanDTO> GetDealKanbanByIdAsync(int dealId)
    {
        var deal = await _db.Deals
            .AsNoTracking()
            .Include(d => d.Contact)
            .Include(d => d.Activities)
            .FirstOrDefaultAsync(d => d.Id == dealId);

        if (deal == null)
            throw new KeyNotFoundException($"Сделка с Id={dealId} не найдена.");

        return new DealKanbanDTO
        {
            Id = deal.Id,
            Name = deal.Name,
            Amount = deal.Amount,
            ContactName = $"{deal.Contact.FirstName} {deal.Contact.LastName}".Trim(),
            CompanyName = deal.Contact.Company,
            ExpectedCloseDate = deal.ExpectedCloseDate,
            ActivitiesCount = deal.Activities.Count
        };
    }

    public async Task<List<SelectListItem>> GetDealsSelectListAsync()
    {
        return await _db.Deals
            .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
            .ToListAsync();
    }

    public async Task<DealDetailsViewModel?> GetDealDetailsAsync(int dealId)
    {
        var deal = await _db.Deals
            .AsNoTracking()
            .Include(d => d.Contact)
            .Include(d => d.Activities)
                .ThenInclude(a => a.Contact)
            .FirstOrDefaultAsync(d => d.Id == dealId);

        if (deal == null)
            return null;

        // Компания через название
        var company = await _db.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == deal.Contact.Company);

        // Все контакты компании
        var contacts = new List<ContactDTO>();
        if (company != null)
        {
            contacts = await _db.Contacts
                .Where(c => c.Company == company.Name)
                .Select(c => new ContactDTO
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    Position = c.Position,
                    CompanyId = company.Id
                })
                .ToListAsync();
        }

        string? companyEmail = contacts.FirstOrDefault()?.Email;

        var activities = deal.Activities.Select(a => new ActivityDTO
        {
            Id = a.Id,
            Type = a.Type,
            Subject = a.Subject,
            Description = a.Description,
            ScheduledDate = a.ScheduledDate > DateTime.MinValue
                ? DateTime.SpecifyKind(a.ScheduledDate, DateTimeKind.Utc)
                : DateTime.SpecifyKind(a.CreatedAt, DateTimeKind.Utc),
            IsCompleted = a.IsCompleted,
            ContactId = a.ContactId,
            DealId = a.DealId,
            CompanyId = company?.Id ?? 0,
            ContactName = a.Contact.FirstName + " " + (a.Contact.LastName ?? ""),
            CompanyName = deal.Contact.Company,
            DealName = deal.Name
        }).OrderByDescending(a => a.ScheduledDate).ToList();

        var manager = deal.AppUserId != null ? await _userManager.FindByIdAsync(deal.AppUserId) : null;

        return new DealDetailsViewModel
        {
            Id = deal.Id,
            Name = deal.Name,
            Amount = deal.Amount,
            Status = deal.Status.ToString(),
            CreatedAt = deal.CreatedAt,
            ExpectedCloseDate = deal.ExpectedCloseDate,
            Description = deal.Description,
            CompanyId = company?.Id,
            CompanyName = deal.Contact.Company,
            CompanyPhone = company?.Phone,
            CompanyEmail = companyEmail,
            ContactFullName = deal.Contact.FirstName + " " + (deal.Contact.LastName ?? ""),
            Contacts = contacts,
            Activities = activities,
            ManagerName = manager != null ? $"{manager.FirstName} {manager.LastName}" : "Не назначен"
        };
    }
}