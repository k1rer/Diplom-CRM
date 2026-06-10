using Diplom_CRM.Data;
using Diplom_CRM.Data.Enums;
using Diplom_CRM.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Activity = Diplom_CRM.Data.Entities.Activity;

namespace Diplom_CRM.Services.Implementations;

public class ActivityService : IActivityService
{
    private readonly AppDbContext _db;

    public ActivityService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ActivityListItemDTO> CreateActivityAsync(ActivityDTO dto)
    {
        var contact = await _db.Contacts.FindAsync(dto.ContactId)
            ?? throw new KeyNotFoundException($"Контакт с Id={dto.ContactId} не найден.");

        if (dto.DealId.HasValue)
        {
            var dealExists = await _db.Deals.AnyAsync(d => d.Id == dto.DealId.Value);
            if (!dealExists)
                throw new KeyNotFoundException($"Сделка с Id={dto.DealId} не найдена.");
        }

        var activity = new Activity
        {
            Type = dto.Type,
            Subject = dto.Subject,
            Description = dto.Description,
            ScheduledDate = dto.ScheduledDate,
            ContactId = dto.ContactId,
            DealId = dto.DealId
        };

        _db.Activities.Add(activity);
        await _db.SaveChangesAsync();

        var dealName = dto.DealId.HasValue
            ? (await _db.Deals.FindAsync(dto.DealId.Value))?.Name
            : null;

        return new ActivityListItemDTO
        {
            Id = activity.Id,
            Type = activity.Type,
            Subject = activity.Subject,
            ScheduledDate = activity.ScheduledDate,
            IsCompleted = activity.IsCompleted,
            ContactName = $"{contact.FirstName} {contact.LastName}".Trim(),
            DealName = dealName
        };
    }

    public async Task ToggleTaskCompletionAsync(int activityId)
    {
        var activity = await _db.Activities.FindAsync(activityId)
            ?? throw new KeyNotFoundException($"Активность с Id={activityId} не найдена.");

        if (activity.Type != TypeEnum.Task)
            throw new InvalidOperationException("Переключать можно только задачи.");

        activity.IsCompleted = !activity.IsCompleted;
        activity.CompletedDate = activity.IsCompleted ? DateTime.UtcNow : null;

        await _db.SaveChangesAsync();
    }

    public Task<List<ActivityListItemDTO>> GetPendingTasksForUserAsync(string userId)
    {
        var today = DateTime.UtcNow.Date;

        return _db.Activities
            .AsNoTracking()
            .Where(a =>
                a.Type == TypeEnum.Task &&
                !a.IsCompleted &&
                a.ScheduledDate.Date <= today)
            .Include(a => a.Contact)
            .Include(a => a.Deal)
            .OrderBy(a => a.ScheduledDate)
            .Select(a => new ActivityListItemDTO
            {
                Id = a.Id,
                Type = a.Type,
                Subject = a.Subject,
                ScheduledDate = a.ScheduledDate,
                IsCompleted = a.IsCompleted,
                ContactName = a.Contact.FirstName + " " + (a.Contact.LastName ?? ""),
                DealName = a.Deal != null ? a.Deal.Name : null
            })
            .ToListAsync();
    }
    public async Task<List<ActivityDTO>> GetActivitiesByCompanyIdAsync(int companyId)
    {
        var company = await _db.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == companyId);

        if (company == null)
            return new List<ActivityDTO>();

        var contactIds = await _db.Contacts
            .Where(c => c.Company == company.Name)
            .Select(c => c.Id)
            .ToListAsync();

        if (!contactIds.Any())
            return new List<ActivityDTO>();

        return await _db.Activities
            .AsNoTracking()
            .Where(a => contactIds.Contains(a.ContactId))
            .OrderByDescending(a => a.ScheduledDate)
            .ThenByDescending(a => a.Id)
            .Select(a => new ActivityDTO
            {
                Id = a.Id,
                Type = a.Type,
                Subject = a.Subject,
                Description = a.Description,
                ScheduledDate = a.ScheduledDate > DateTime.MinValue ? a.ScheduledDate : a.CreatedAt,
                ContactId = a.ContactId,
                DealId = a.DealId,
                CompanyId = companyId,
                IsCompleted = a.IsCompleted
            })
            .ToListAsync();
    }

    public async Task DeleteActivityAsync(int activityId)
    {
        var activity = await _db.Activities.FindAsync(activityId);
        if (activity != null)
        {
            _db.Activities.Remove(activity);
            await _db.SaveChangesAsync();
        }
    }
}