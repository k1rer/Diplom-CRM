namespace Diplom_CRM.Models.View
{
    public class DeleteContactConfirmationViewModel
    {
        public DTO.ContactDTO Contact { get; set; } = new();
        public bool HasActivities { get; set; }
    }
}
