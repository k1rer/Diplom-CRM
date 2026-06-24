using Diplom_CRM.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Diplom_CRM.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;


        public ICollection<Deal> Deals { get; set; } = new List<Deal>();
        public ICollection<Company> Companies { get; set; } = new List<Company>();
    }
}
