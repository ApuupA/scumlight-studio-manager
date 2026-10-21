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

        // ---------- Mixing (song page, only with scope "Mixing") ----------

        public DateOnly? MaterialReceivedOn { get; set; }

        /// <summary>Where the multitracks / stems are, e.g. a WeTransfer or Nextcloud link.</summary>
        public string? MaterialLink { get; set; }

        public int? MaterialTrackCount { get; set; }

        /// <summary>Problems found in the material, e.g. "Bass clipped in the chorus".</summary>
        public string? MaterialNotes { get; set; }

        /// <summary>Mix recall: plugin chain, bus settings, ...</summary>
        public string? MixNotes { get; set; }

        // ---------- Mastering (only with scope "Mastering") ----------

        public string? Isrc { get; set; }
        public decimal? MeasuredLufs { get; set; }
        public decimal? TruePeak { get; set; }

        /// <summary>Pause before the song on the album in seconds.</summary>
        public decimal? GapBeforeSeconds { get; set; }

        public ICollection<MixVersion> MixVersions { get; set; } = new List<MixVersion>();

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
