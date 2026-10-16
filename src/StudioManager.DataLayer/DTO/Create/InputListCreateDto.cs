using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    public class InputListCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        /// <summary>Position of the tab. 0 = append at the end.</summary>
        public int SortOrder { get; set; }

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }
    }
}
