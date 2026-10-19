using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// One part of a song's arrangement. SortOrder 0 = append at the end.
    /// </summary>
    public class SongSectionCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 99999)]
        public int? FromBar { get; set; }

        [Range(0, 99999)]
        public int? ToBar { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public int SortOrder { get; set; }

        [Range(1, int.MaxValue)]
        public int SongId { get; set; }
    }
}
