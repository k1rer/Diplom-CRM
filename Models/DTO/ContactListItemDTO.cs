namespace Diplom_CRM.Models.DTO
{
    public class ContactListItemDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Position { get; set; }
    }
}
