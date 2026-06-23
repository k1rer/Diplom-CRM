namespace Diplom_CRM.Models.DTO
{
    public class DealSummaryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }
}
