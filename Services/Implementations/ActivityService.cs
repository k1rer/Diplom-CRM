using Diplom_CRM.Data;
using Diplom_CRM.Data.Enums;
using Diplom_CRM.Models.DTO;
using Diplom_CRM.Models.View;
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

        if (dto.ScheduledDate == default || dto.ScheduledDate == DateTime.MinValue)
        {
            dto.ScheduledDate = DateTime.UtcNow;
        }
        else
        {
            if (dto.ScheduledDate.Kind == DateTimeKind.Unspecified)
            {
                dto.ScheduledDate = DateTime.SpecifyKind(dto.ScheduledDate, DateTimeKind.Local).ToUniversalTime();
            }
            else if (dto.ScheduledDate.Kind == DateTimeKind.Local)
            {
                dto.ScheduledDate = dto.ScheduledDate.ToUniversalTime();
            }
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
                IsCompleted = a.IsCompleted,
                CompletedDate = a.CompletedDate
            })
            .ToListAsync();
    }

    public async Task DeleteActivityAsync(int activityId)
    {
        var activity = await _db.Activities.FindAsync(activityId);
        if (activity == null) 
            throw new KeyNotFoundException($"Активность с Id={activityId} не найдена.");
        _db.Activities.Remove(activity);
        await _db.SaveChangesAsync();
    }

    public async Task<List<ActivityDTO>> GetFilteredActivitiesAsync(ActivityFilterViewModel filter)
    {
        var query = _db.Activities
            .AsNoTracking()
            .Include(a => a.Contact)
            .Include(a => a.Deal)
            .AsQueryable();

        // Фильтр по типу
        if (!string.IsNullOrWhiteSpace(filter.Type) && Enum.TryParse<TypeEnum>(filter.Type, out var typeEnum))
            query = query.Where(a => a.Type == typeEnum);

        // Фильтр по компании
        if (filter.CompanyId.HasValue)
        {
            var company = await _db.Companies.FindAsync(filter.CompanyId.Value);
            if (company != null)
                query = query.Where(a => a.Contact.Company == company.Name);
        }

        // Фильтр по сделке
        if (filter.DealId.HasValue)
            query = query.Where(a => a.DealId == filter.DealId.Value);

        // Статус выполнения
        if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status != "All")
        {
            if (filter.Status == "Pending")
                query = query.Where(a => !a.IsCompleted);
            else if (filter.Status == "Completed")
                query = query.Where(a => a.IsCompleted);
        }

        // Период
        if (filter.FromDate.HasValue)
            query = query.Where(a => a.ScheduledDate >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(a => a.ScheduledDate <= filter.ToDate.Value);

        // Поиск по теме и описанию
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(a => a.Subject.ToLower().Contains(term) ||
                                     (a.Description != null && a.Description.ToLower().Contains(term)));
        }

        if (filter.CompanyId.HasValue)
        {
            var company = await _db.Companies.FindAsync(filter.CompanyId.Value);
            if (company != null)
                query = query.Where(a => a.Contact.Company == company.Name);
        }

        if (filter.DealId.HasValue)
            query = query.Where(a => a.DealId == filter.DealId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status) && filter.Status != "All")
        {
            if (filter.Status == "Pending")
                query = query.Where(a => !a.IsCompleted);
            else if (filter.Status == "Completed")
                query = query.Where(a => a.IsCompleted);
        }

        if (filter.FromDate.HasValue)
            query = query.Where(a => a.ScheduledDate >= filter.FromDate.Value);
        if (filter.ToDate.HasValue)
            query = query.Where(a => a.ScheduledDate <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(a => a.Subject.ToLower().Contains(term) ||
                                     (a.Description != null && a.Description.ToLower().Contains(term)));
        }

        query = filter.SortBy switch
        {
            "date_asc" => query.OrderBy(a => a.ScheduledDate),
            _ => query.OrderByDescending(a => a.ScheduledDate)
        };

        var activities = await query.Select(a => new ActivityDTO
        {
            Id = a.Id,
            Type = a.Type,
            Subject = a.Subject,
            Description = a.Description,
            ScheduledDate = a.ScheduledDate,
            IsCompleted = a.IsCompleted,
            ContactId = a.ContactId,
            DealId = a.DealId,
            ContactName = a.Contact.FirstName + " " + (a.Contact.LastName ?? ""),
            CompanyName = a.Contact.Company,
            DealName = a.Deal != null ? a.Deal.Name : null,
            CompletedDate = a.CompletedDate.HasValue
                ? DateTime.SpecifyKind(a.CompletedDate.Value, DateTimeKind.Utc)
                : null
        }).ToListAsync();

        var companyNames = activities.Select(a => a.CompanyName).Where(n => n != null).Distinct().ToList();
        if (companyNames.Any())
        {
            var companyDict = await _db.Companies
                .Where(c => companyNames.Contains(c.Name))
                .ToDictionaryAsync(c => c.Name!, c => c.Id);
            foreach (var a in activities)
                if (a.CompanyName != null && companyDict.ContainsKey(a.CompanyName))
                    a.CompanyId = companyDict[a.CompanyName];
        }

        return activities;
    }
    public async Task<ActivityDTO?> GetActivityByIdAsync(int activityId)
    {
        return await _db.Activities
            .AsNoTracking()
            .Include(a => a.Contact)
            .Include(a => a.Deal)
            .Where(a => a.Id == activityId)
            .Select(a => new ActivityDTO
            {
                Id = a.Id,
                Type = a.Type,
                Subject = a.Subject,
                Description = a.Description,
                ScheduledDate = a.ScheduledDate > DateTime.MinValue
                    ? DateTime.SpecifyKind(a.ScheduledDate, DateTimeKind.Utc)
                    : DateTime.SpecifyKind(a.CreatedAt, DateTimeKind.Utc),
                CompletedDate = a.CompletedDate.HasValue
                    ? DateTime.SpecifyKind(a.CompletedDate.Value, DateTimeKind.Utc)
                    : null,
                IsCompleted = a.IsCompleted,
                ContactId = a.ContactId,
                DealId = a.DealId,
                CompanyId = _db.Companies
                    .Where(c => c.Name == a.Contact.Company)
                    .Select(c => c.Id)
                    .FirstOrDefault(),
                ContactName = a.Contact.FirstName + " " + (a.Contact.LastName ?? ""),
                CompanyName = a.Contact.Company,
                DealName = a.Deal != null ? a.Deal.Name : null
            })
            .FirstOrDefaultAsync();
    }
}