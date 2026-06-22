using System.ComponentModel.DataAnnotations;

namespace Diplom_CRM.Data.Enums
{
    public enum TypeEnum
    {
        [Display(Name = "Звонок")]
        Call,
        [Display(Name = "Email")]
        Email,
        [Display(Name = "Встреча")]
        Meeting,
        [Display(Name = "Задача")]
        Task
    }
}
