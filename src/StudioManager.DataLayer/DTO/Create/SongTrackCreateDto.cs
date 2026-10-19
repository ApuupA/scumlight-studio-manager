using System.ComponentModel.DataAnnotations;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// One entry of a song's recording checklist. SortOrder 0 = append at the end.
    /// </summary>
    public class SongTrackCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public SongTrackStatus Status { get; set; } = SongTrackStatus.Open;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public int SortOrder { get; set; }

        [Range(1, int.MaxValue)]
        public int SongId { get; set; }
    }
}
