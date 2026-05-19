using Diplom_CRM.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diplom_CRM.Data.Entities
{
    public class Deal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Precision(18, 2)]
        public decimal Amount { get; set; }

        public StatusEnum Status { get; set; }

        public DateTime ExpectedCloseDate { get; set; }

        public DateTime? ActualCloseDate { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        public int ContactId { get; set; }


        public Contact Contact { get; set; } = null!;

        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }
}