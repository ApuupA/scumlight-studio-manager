using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    public class StudioInputCreateDto
    {
        /// <summary>Position in the input list. 0 = append at the end.</summary>
        public int SortOrder { get; set; }

        [MaxLength(50)]
        public string? SnakeChannel { get; set; }

        [MaxLength(100)]
        public string? Preamp { get; set; }

        [MaxLength(50)]
        public string? DawChannel { get; set; }

        [MaxLength(200)]
        public string? Notes { get; set; }

        public int? InputGroupId { get; set; }
    }
}
