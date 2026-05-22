using Diplom_CRM.Data.Enums;

namespace Diplom_CRM.Models.DTO
{
    public class ActivityDTO
    {
        public TypeEnum Type { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime ScheduledDate { get; set; }
        public int ContactId { get; set; }
        public int? DealId { get; set; }
    }
}
