using Diplom_CRM.Models.DTO;

namespace Diplom_CRM.Models.View
{
    public class KanbanViewModel
    {
        public Dictionary<string, List<DealKanbanDTO>> Columns { get; set; } = new();
    }
}
