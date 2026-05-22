using Diplom_CRM.Models.DTO;

namespace Diplom_CRM.Services
{
    public interface IClientService
    {
        Task<PagedResultDTO<CompanyListItemDTO>> GetPagedCompaniesAsync(int page, int pageSize, string? searchTerm);
        Task<CompanyListItemDTO> CreateCompanyAsync(CompanyDTO dto);
        Task<ContactListItemDTO> AddContactToCompanyAsync(int companyId, ContactDTO dto);
        Task<List<TimelineItemDTO>> GetCompanyTimelineAsync(int companyId);
    }
}
