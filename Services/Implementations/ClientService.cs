using Diplom_CRM.Data;
using Diplom_CRM.Data.Entities;
using Diplom_CRM.Exceptions;
using Diplom_CRM.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Diplom_CRM.Services.Implementations;

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _db;

    public ClientService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResultDTO<CompanyListItemDTO>> GetPagedCompaniesAsync(
        int page, int pageSize, string? searchTerm)
    {
        var query = _db.Companies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                (c.Phone != null && c.Phone.Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CompanyListItemDTO
            {
                Id = c.Id,
                Name = c.Name,
                Industry = c.Industry,
                City = c.City,
                Country = c.Country,
                ContactsCount = c.Contacts.Count,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PagedResultDTO<CompanyListItemDTO>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CompanyListItemDTO> CreateCompanyAsync(CompanyDTO dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            var phoneExists = await _db.Companies.AnyAsync(c => c.Phone == dto.Phone);
            if (phoneExists)
                throw new DuplicateEntityException($"Компания с телефоном {dto.Phone} уже существует.");
        }

        var company = new Company
        {
            Name = dto.Name,
            Industry = dto.Industry,
            Website = dto.Website,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country
        };

        _db.Companies.Add(company);
        await _db.SaveChangesAsync();

        return new CompanyListItemDTO
        {
            Id = company.Id,
            Name = company.Name,
            Industry = company.Industry,
            City = company.City,
            Country = company.Country,
            ContactsCount = 0,
            CreatedAt = company.CreatedAt
        };
    }

    public async Task<ContactListItemDTO> AddContactToCompanyAsync(int companyId, ContactDTO dto)
    {
        var company = await _db.Companies.FindAsync(companyId)
            ?? throw new KeyNotFoundException($"Компания с Id={companyId} не найдена.");

        var contact = new Contact
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Position = dto.Position,
            Company = company.Name 
        };

        _db.Contacts.Add(contact);
        await _db.SaveChangesAsync();

        return new ContactListItemDTO
        {
            Id = contact.Id,
            FullName = $"{contact.FirstName} {contact.LastName}".Trim(),
            Email = contact.Email,
            Phone = contact.Phone,
            Position = contact.Position
        };
    }

    public async Task<List<TimelineItemDTO>> GetCompanyTimelineAsync(int companyId)
    {
        var company = await _db.Companies
            .AsNoTracking()
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == companyId)
            ?? throw new KeyNotFoundException($"Компания с Id={companyId} не найдена.");

        var contactIds = company.Contacts.Select(c => c.Id).ToList();
        if (!contactIds.Any())
            return new List<TimelineItemDTO>();

        var deals = await _db.Deals
            .AsNoTracking()
            .Where(d => contactIds.Contains(d.ContactId))
            .Select(d => new TimelineItemDTO
            {
                Date = d.CreatedAt,
                Type = "Deal",
                Description = $"Сделка: {d.Name}",
                Detail = $"Сумма: {d.Amount:C}, Статус: {d.Status}"
            })
            .ToListAsync();

        var activities = await _db.Activities
            .AsNoTracking()
            .Where(a => contactIds.Contains(a.ContactId))
            .Select(a => new TimelineItemDTO
            {
                Date = a.ScheduledDate,
                Type = "Activity",
                Description = $"{a.Type}: {a.Subject}",
                Detail = a.IsCompleted ? "Выполнено" : "Запланировано"
            })
            .ToListAsync();

        return deals.Concat(activities)
            .OrderByDescending(t => t.Date)
            .ToList();
    }
}