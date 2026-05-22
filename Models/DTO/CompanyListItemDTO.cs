namespace Diplom_CRM.Models.DTO
{
    public class CompanyListItemDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public int ContactsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
