using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    public class InputGroupCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>Color name of the theme palette, e.g. "orange".</summary>
        [Required, MaxLength(20)]
        public string Color { get; set; } = "gray";

        public int SortOrder { get; set; }
    }
}
