namespace Diplom_CRM.Models.DTO
{
    public class DealKanbanDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string? CompanyName { get; set; }
        public DateTime ExpectedCloseDate { get; set; }
    }
}
