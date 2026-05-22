using Diplom_CRM.Data;
using Diplom_CRM.Data.Entities;
using Diplom_CRM.Data.Enums;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Services;
using Microsoft.EntityFrameworkCore;
using Activity = Diplom_CRM.Data.Entities.Activity;

namespace Diplom_CRM.Services.Implementations;

public class ActivityService : IActivityService
{
    private readonly ApplicationDbContext _db;

    public ActivityService(ApplicationDbContext db)
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

    public async Task SetTaskAsCompletedAsync(int activityId)
    {
        var activity = await _db.Activities.FindAsync(activityId)
            ?? throw new KeyNotFoundException($"Активность с Id={activityId} не найдена.");

        if (activity.Type != TypeEnum.Task)
            throw new InvalidOperationException("Завершить можно только активность с типом «Задача».");

        activity.IsCompleted = true;
        activity.CompletedDate = DateTime.UtcNow;

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
}