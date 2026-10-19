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

        /// <summary>Instrument tuning, e.g. "Drop C" or "Eb standard".</summary>
        public string? Tuning { get; set; }

        public string? Notes { get; set; }

        /// <summary>Lyrics, e.g. for the vocal booth.</summary>
        public string? Lyrics { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        /// <summary>
        /// The band playing this song - important for split releases. Null if the project has only one band.
        /// </summary>
        public int? BandId { get; set; }
        public Band? Band { get; set; }

        /// <summary>
        /// Tempo(s) of the song in song order - several if the tempo changes.
        /// </summary>
        public ICollection<SongTempo> Tempos { get; set; } = new List<SongTempo>();

        /// <summary>Arrangement: Intro, Verse 1, Chorus 1, ...</summary>
        public ICollection<SongSection> Sections { get; set; } = new List<SongSection>();

        /// <summary>Recording checklist: the tracks of the song and their progress.</summary>
        public ICollection<SongTrack> Tracks { get; set; } = new List<SongTrack>();

        /// <summary>
        /// Sessions in which this song was worked on.
        /// </summary>
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
