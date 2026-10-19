using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// An extra item of a project, e.g. "Tape reel" 2 × 45 EUR.
    /// </summary>
    public class ProjectCostItemCreateDto
    {
        public DateOnly Date { get; set; }

        [Required, MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 100000)]
        public decimal Quantity { get; set; } = 1;

        [Range(0, 1000000)]
        public decimal UnitPrice { get; set; }

        public bool IsBillable { get; set; } = true;

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }
    }
}
