using Diplom_CRM.Models.DTO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Diplom_CRM.Services
{
    public interface IClientService
    {
        Task<PagedResultDTO<CompanyListItemDTO>> GetPagedCompaniesAsync(int page, int pageSize, string? searchTerm);
        Task<CompanyListItemDTO> CreateCompanyAsync(CompanyDTO dto);
        Task<ContactListItemDTO> AddContactToCompanyAsync(int companyId, ContactDTO dto);
        Task<List<TimelineItemDTO>> GetCompanyTimelineAsync(int companyId);
        Task<CompanyDTO?> GetCompanyByIdAsync(int companyId);
        Task<List<ContactDTO>> GetContactsByCompanyIdAsync(int companyId);
        Task DeleteContactAsync(int contactId);
        Task<ContactDTO?> GetContactByIdAsync(int contactId);
        Task UpdateContactAsync(ContactDTO dto);
        Task<bool> ContactHasActivitiesAsync(int contactId);
        Task<List<SelectListItem>> GetCompaniesSelectListAsync();
    }
}
