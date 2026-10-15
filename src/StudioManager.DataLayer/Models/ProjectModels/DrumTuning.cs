namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// Tuning of one drum, measured e.g. with a TuneBot. Frequencies in Hz - the frontend shows the matching note.
    /// </summary>
    public class DrumTuning : BaseEntity
    {
        /// <summary>The drum, e.g. "Snare 14x6.5" or "Floor tom 16".</summary>
        public string Drum { get; set; } = string.Empty;

        public decimal? BatterHz { get; set; }
        public decimal? ResonantHz { get; set; }

        /// <summary>Drum heads used, e.g. "Remo Emperor coated / Ambassador clear".</summary>
        public string? Heads { get; set; }

        public string? Notes { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        /// <summary>
        /// Optional: a song specific tuning. Null = tuning for the whole project.
        /// </summary>
        public int? SongId { get; set; }
        public Song? Song { get; set; }
    }
}
