using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Input for creating (POST) and updating (PUT) a gear category.
    /// </summary>
    public class GearCategoryCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }

        /// <summary>Software and plugins - never "in session today".</summary>
        public bool IsSoftware { get; set; }
    }
}
