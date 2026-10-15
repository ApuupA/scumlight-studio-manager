using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.SessionModels;

namespace StudioManager.DataLayer.Models.ProjectModels
{
    public class Song : BaseEntity
    {
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Position on the release (track list order).
        /// </summary>
        public int TrackNumber { get; set; }

        public SongStatus Status { get; set; } = SongStatus.Planned;
        public decimal? Bpm { get; set; }

        /// <summary>Musical key, e.g. "F# minor".</summary>
        public string? Key { get; set; }

        /// <summary>Instrument tuning, e.g. "Drop C" or "Eb standard".</summary>
        public string? Tuning { get; set; }

        public string? Notes { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        /// <summary>
        /// The band playing this song - important for split releases. Null if the project has only one band.
        /// </summary>
        public int? BandId { get; set; }
        public Band? Band { get; set; }

        /// <summary>
        /// Sessions in which this song was worked on.
        /// </summary>
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
