using Diplom_CRM.Models.DTO;

namespace Diplom_CRM.Models.View
{
    public class CreateDealViewModel
    {
        public DealDTO Deal { get; set; } = new();
        public List<ContactDTO> Contacts { get; set; } = new();
        public bool HasContacts => Contacts != null && Contacts.Any();
    }
}
