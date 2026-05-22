using Diplom_CRM.Data.Enums;

namespace Diplom_CRM.Models.DTO
{
    public class ActivityListItemDTO
    {
        public int Id { get; set; }
        public TypeEnum Type { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime ScheduledDate { get; set; }
        public bool IsCompleted { get; set; }
        public string? ContactName { get; set; }
        public string? DealName { get; set; }
    }
}
