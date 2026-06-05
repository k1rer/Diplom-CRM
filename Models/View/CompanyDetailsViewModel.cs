using Diplom_CRM.Models.DTO;

namespace Diplom_CRM.Models.View
{
    public class CompanyDetailsViewModel
    {
        public CompanyDTO? Company { get; set; }
        public List<ContactDTO>? Contacts { get; set; }
        public List<ActivityDTO>? Activities { get; set; }
    }
}
