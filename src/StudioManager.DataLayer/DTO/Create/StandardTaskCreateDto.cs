using System.ComponentModel.DataAnnotations;
using StudioManager.DataLayer.Models.FinanceModels;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// A standard task (settings), e.g. "Recording / Tag" with 8 hours per day. SortOrder 0 = append at the end.
    /// </summary>
    public class StandardTaskCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public TaskUnit Unit { get; set; } = TaskUnit.Hour;

        /// <summary>Hours of one day (unit Day). For the unit Hour it is always 1.</summary>
        [Range(0.25, 24)]
        public decimal HoursPerUnit { get; set; } = 1;

        public int SortOrder { get; set; }

        public bool IsArchived { get; set; }
    }
}
