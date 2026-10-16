using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.SessionModels;

namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// A production (album, EP, ...) for a client. Collects everything about recording, mixing and mastering.
    /// </summary>
    public class Project : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public ProjectType Type { get; set; } = ProjectType.Album;
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
        public DateOnly? StartDate { get; set; }

        /// <summary>
        /// Deadline, e.g. the release date or when the master has to be delivered.
        /// </summary>
        public DateOnly? DueDate { get; set; }

        public string? Description { get; set; }

        // ---------- Technical settings of the production ----------
        /// <summary>Sample rate in Hz, e.g. 48000.</summary>
        public int? SampleRate { get; set; }

        /// <summary>Bit depth, e.g. 24.</summary>
        public int? BitDepth { get; set; }

        /// <summary>DAW / template used, e.g. "Cubase 14 - Band template".</summary>
        public string? Daw { get; set; }

        /// <summary>Loudness target for the master in LUFS (integrated), e.g. -9.</summary>
        public decimal? TargetLufs { get; set; }

        // ---------- Notes per production phase ----------
        public string? RecordingNotes { get; set; }
        public string? MixingNotes { get; set; }
        public string? MasteringNotes { get; set; }

        public int ClientId { get; set; }
        public Client? Client { get; set; }

        /// <summary>
        /// Bands of the client that take part (many-to-many - a split EP has several).
        /// </summary>
        public ICollection<Band> Bands { get; set; } = new List<Band>();

        public ICollection<Song> Songs { get; set; } = new List<Song>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<ProjectLink> Links { get; set; } = new List<ProjectLink>();
        public ICollection<DrumTuning> DrumTunings { get; set; } = new List<DrumTuning>();
        public ICollection<ExternalGear> ExternalGear { get; set; } = new List<ExternalGear>();
        public ICollection<ProjectInstrument> Instruments { get; set; } = new List<ProjectInstrument>();
    }
}
