using Diplom_CRM.Models.DTO;

namespace Diplom_CRM.Models.View
{
    public class DealDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpectedCloseDate { get; set; }
        public string? Description { get; set; }

        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyPhone { get; set; }
        public string? CompanyEmail { get; set; }

        public string? ContactFullName { get; set; }

        public List<ContactDTO> Contacts { get; set; } = new();

        public List<ActivityDTO> Activities { get; set; } = new();

        public string? From { get; set; }

        public string? ManagerName { get; set; }
    }
}