using System.ComponentModel.DataAnnotations;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Create
{
    public class SongCreateDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Range(0, 999)]
        public int TrackNumber { get; set; }

        public SongStatus Status { get; set; } = SongStatus.Planned;

        [Range(1, 400)]
        public decimal? Bpm { get; set; }

        [MaxLength(50)]
        public string? Key { get; set; }

        [MaxLength(50)]
        public string? Tuning { get; set; }

        [MaxLength(4000)]
        public string? Notes { get; set; }

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        public int? BandId { get; set; }
    }
}
