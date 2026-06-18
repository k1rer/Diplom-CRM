namespace Diplom_CRM.Models.View
{
    public class ActivityFilterViewModel
    {
        public string? Type { get; set; }
        public int? CompanyId { get; set; }
        public int? DealId { get; set; }
        public string? Status { get; set; }
        public string? UserId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; }
    }
}
