using Diplom_CRM.Data;
using Diplom_CRM.Data.Enums;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;
using Microsoft.EntityFrameworkCore;
using Activity = Diplom_CRM.Data.Entities.Activity;

namespace Diplom_CRM.Services.Implementations;

public class ActivityService(ApplicationDbContext db) : IActivityService
{
    public async Task<ActivityListItemDTO> CreateActivityAsync(ActivityDTO dto)
    {
        var contact = await db.Contacts
            .AsNoTracking()
            .Select(c => new { c.Id, c.FirstName, c.LastName })
            .FirstOrDefaultAsync(c => c.Id == dto.ContactId)
            ?? throw new KeyNotFoundException($"Контакт с Id={dto.ContactId} не найден.");

        string? dealName = null;
        if (dto.DealId.HasValue)
        {
            dealName = await db.Deals
                .AsNoTracking()
                .Where(d => d.Id == dto.DealId.Value)
                .Select(d => d.Name)
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException($"Сделка с Id={dto.DealId} не найдена.");
        }

        // Нормализация даты под UTC (для PostgreSQL)
        var scheduledDate = dto.ScheduledDate == default || dto.ScheduledDate == DateTime.MinValue
            ? DateTime.UtcNow
            : DateTime.SpecifyKind(dto.ScheduledDate, DateTimeKind.Utc);

        var activity = new Activity
        {
            Type = dto.Type,
            Subject = dto.Subject,
            Description = dto.Description,
            ScheduledDate = scheduledDate,
            IsCompleted = dto.IsCompleted,
            ContactId = dto.ContactId,
            DealId = dto.DealId
        };

        db.Activities.Add(activity);
        await db.SaveChangesAsync();

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
        var activity = await db.Activities.FindAsync(activityId)
            ?? throw new KeyNotFoundException($"Активность с Id={activityId} не найдена.");

        if (activity.Type != TypeEnum.Task)
            throw new InvalidOperationException("Переключать можно только задачи.");

        activity.IsCompleted = !activity.IsCompleted;
        activity.CompletedDate = activity.IsCompleted ? DateTime.UtcNow : null;

        await db.SaveChangesAsync();
    }

    public Task<List<ActivityListItemDTO>> GetPendingTasksForUserAsync(string userId)
    {
        var today = DateTime.UtcNow.Date;

        return db.Activities
            .AsNoTracking()
            .Where(a => a.Type == TypeEnum.Task &&
                        !a.IsCompleted &&
                        a.ScheduledDate.Date <= today)
            .OrderBy(a => a.ScheduledDate)
            .Select(a => new ActivityListItemDTO
            {
                Id = a.Id,
                Type = a.Type,
                Subject = a.Subject,
                ScheduledDate = a.ScheduledDate,
                IsCompleted = a.IsCompleted,
                ContactName = (a.Contact.FirstName + " " + (a.Contact.LastName ?? "")).Trim(),
                DealName = a.Deal != null ? a.Deal.Name : null
            })
            .ToListAsync();
    }

    public async Task<List<ActivityDTO>> GetActivitiesByCompanyIdAsync(int companyId)
    {
        var companyName = await db.Companies
            .AsNoTracking()
            .Where(c => c.Id == companyId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(companyName))
            return [];

        return await db.Activities
            .AsNoTracking()
            .Where(a => a.Contact.Company == companyName)
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
                IsCompleted = a.IsCompleted,
                CompletedDate = a.CompletedDate
            })
            .ToListAsync();
    }

    public async Task DeleteActivityAsync(int activityId)
    {
        var activity = await db.Activities.FindAsync(activityId)
            ?? throw new KeyNotFoundException($"Активность с Id={activityId} не найдена.");

        db.Activities.Remove(activity);
        await db.SaveChangesAsync();
    }

    public async Task<List<ActivityDTO>> GetFilteredActivitiesAsync(ActivityFilterViewModel filter)
    {
        var query = db.Activities
            .AsNoTracking()
            .AsQueryable();

        // 1. Фильтр по типу
        if (!string.IsNullOrWhiteSpace(filter.Type) && Enum.TryParse<TypeEnum>(filter.Type, out var typeEnum))
            query = query.Where(a => a.Type == typeEnum);

        // 2. Фильтр по компании
        if (filter.CompanyId.HasValue)
        {
            var companyName = await db.Companies
                .Where(c => c.Id == filter.CompanyId.Value)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(companyName))
                query = query.Where(a => a.Contact.Company == companyName);
        }

        // 3. Фильтр по сделке
        if (filter.DealId.HasValue)
            query = query.Where(a => a.DealId == filter.DealId.Value);

        // 4. Статус выполнения
        if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status != "All")
        {
            if (filter.Status == "Pending")
                query = query.Where(a => !a.IsCompleted);
            else if (filter.Status == "Completed")
                query = query.Where(a => a.IsCompleted);
        }

        // 5. Период дат
        if (filter.FromDate.HasValue)
            query = query.Where(a => a.ScheduledDate >= DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Utc));
        if (filter.ToDate.HasValue)
            query = query.Where(a => a.ScheduledDate <= DateTime.SpecifyKind(filter.ToDate.Value, DateTimeKind.Utc));

        // 6. Поиск по теме и описанию (использование ILike в PostgreSQL для регистронезависимости)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(a => EF.Functions.ILike(a.Subject, $"%{term}%") ||
                                     (a.Description != null && EF.Functions.ILike(a.Description, $"%{term}%")));
        }

        // 7. Сортировка
        query = filter.SortBy switch
        {
            "date_asc" => query.OrderBy(a => a.ScheduledDate),
            _ => query.OrderByDescending(a => a.ScheduledDate)
        };

        return await query.Select(a => new ActivityDTO
        {
            Id = a.Id,
            Type = a.Type,
            Subject = a.Subject,
            Description = a.Description,
            ScheduledDate = a.ScheduledDate,
            IsCompleted = a.IsCompleted,
            ContactId = a.ContactId,
            DealId = a.DealId,
            ContactName = (a.Contact.FirstName + " " + (a.Contact.LastName ?? "")).Trim(),
            CompanyName = a.Contact.Company,
            DealName = a.Deal != null ? a.Deal.Name : null,
            CompletedDate = a.CompletedDate
        }).ToListAsync();
    }

    public async Task<ActivityDTO?> GetActivityByIdAsync(int activityId)
    {
        return await db.Activities
            .AsNoTracking()
            .Where(a => a.Id == activityId)
            .Select(a => new ActivityDTO
            {
                Id = a.Id,
                Type = a.Type,
                Subject = a.Subject,
                Description = a.Description,
                ScheduledDate = a.ScheduledDate > DateTime.MinValue ? a.ScheduledDate : a.CreatedAt,
                CompletedDate = a.CompletedDate,
                IsCompleted = a.IsCompleted,
                ContactId = a.ContactId,
                DealId = a.DealId,
                ContactName = (a.Contact.FirstName + " " + (a.Contact.LastName ?? "")).Trim(),
                CompanyName = a.Contact.Company,
                DealName = a.Deal != null ? a.Deal.Name : null
            })
            .FirstOrDefaultAsync();
    }
}