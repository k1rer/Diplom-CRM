using System.ComponentModel.DataAnnotations;

namespace Diplom_CRM.Data.Enums
{
    public enum StatusEnum
    {
        [Display(Name = "Новая")]
        New,
        [Display(Name = "В работе")]
        InProgress,
        [Display(Name = "Выигранная")]
        Won,
        [Display(Name = "Проигранная")]
        Lost
    }
}
