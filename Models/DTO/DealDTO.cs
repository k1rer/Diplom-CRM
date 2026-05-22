namespace Diplom_CRM.Models.DTO
{
    public class DealDTO
    {
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpectedCloseDate { get; set; }
        public string? Description { get; set; }
        public int ContactId { get; set; }
    }
}
