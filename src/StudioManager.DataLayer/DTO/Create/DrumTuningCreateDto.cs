using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    public class DrumTuningCreateDto
    {
        [Required, MaxLength(100)]
        public string Drum { get; set; } = string.Empty;

        /// <summary>The fundamental the drum is tuned to.</summary>
        [Range(20, 2000)]
        public decimal? FundamentalHz { get; set; }

        [Range(20, 2000)]
        public decimal? BatterHz { get; set; }

        [Range(20, 2000)]
        public decimal? ResonantHz { get; set; }

        [MaxLength(200)]
        public string? Heads { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        public int? SongId { get; set; }
    }
}
