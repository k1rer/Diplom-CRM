namespace Diplom_CRM.Models.DTO
{
    public class TimelineItemDTO
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;  // "Deal" или "Activity"
        public string Description { get; set; } = string.Empty;
        public string? Detail { get; set; }
    }
}
