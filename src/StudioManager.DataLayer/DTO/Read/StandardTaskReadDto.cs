using StudioManager.DataLayer.Models.FinanceModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class StandardTaskReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TaskUnit Unit { get; set; }
        public decimal HoursPerUnit { get; set; }
        public int SortOrder { get; set; }
        public bool IsArchived { get; set; }
    }
}
