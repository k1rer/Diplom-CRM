using Diplom_CRM.Data.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diplom_CRM.Data.Entities
{
    public class Activity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public TypeEnum Type { get; set; }

        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public DateTime ScheduledDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public int ContactId { get; set; }

        public int? DealId { get; set; }


        public Contact Contact { get; set; } = null!;

        public Deal? Deal { get; set; }
    }
}
