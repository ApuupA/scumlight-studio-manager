using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.SessionModels;

namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// An instrument (or voice) recorded in the project, e.g. "Drums", "Guitar left", "Lead vocals".
    /// Its setup lists the gear used for it - sessions choose instruments and get their gear from there.
    /// </summary>
    public class ProjectInstrument : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// True: the setup is a signal chain and shown with arrows (guitar -> pedal -> amp -> cab -> mic).
        /// False: the setup is a plain list (e.g. the mics of a drum kit).
        /// </summary>
        public bool IsChain { get; set; }

        /// <summary>Position in the list of instruments (ascending).</summary>
        public int SortOrder { get; set; }

        public string? Notes { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        /// <summary>Optional: the band playing it (interesting for split releases).</summary>
        public int? BandId { get; set; }
        public Band? Band { get; set; }

        public ICollection<InstrumentSetupItem> SetupItems { get; set; } = new List<InstrumentSetupItem>();

        /// <summary>
        /// Sessions in which this instrument is recorded (many-to-many).
        /// </summary>
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
