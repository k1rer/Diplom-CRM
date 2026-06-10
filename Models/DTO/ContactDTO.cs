namespace Diplom_CRM.Models.DTO
{
    public class ContactDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Position { get; set; }
        public int CompanyId { get; set; }
    }
}
