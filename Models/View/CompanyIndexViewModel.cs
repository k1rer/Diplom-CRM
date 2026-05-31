using Diplom_CRM.Models.DTO;

namespace Diplom_CRM.Models.View
{
    public class CompanyIndexViewModel
    {
        public PagedResultDTO<CompanyListItemDTO> Companies { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int CurrentPage { get; set; } = 1;
    }
}
