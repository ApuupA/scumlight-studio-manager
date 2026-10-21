namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// One mix export of the project, e.g. "v1" (all songs) or "v1.1" (only the songs with changes).
    /// The client reviews it and gives feedback; every further version of a song is a revision of that song.
    /// </summary>
    public class MixVersion : BaseEntity
    {
        /// <summary>Order of the versions in the project (1, 2, 3, ...) - given automatically.</summary>
        public int Number { get; set; }

        /// <summary>The name shown to everyone, e.g. "v1", "v1.1", "v2 Final".</summary>
        public string Name { get; set; } = string.Empty;

        public DateOnly Date { get; set; }

        public MixVersionStatus Status { get; set; } = MixVersionStatus.Sent;

        /// <summary>Where the export is, e.g. a Nextcloud share link.</summary>
        public string? Link { get; set; }

        /// <summary>What changed in this version, e.g. "Vocals +1 dB on all songs".</summary>
        public string? Notes { get; set; }

        /// <summary>The name of the client who approved the mix on the client page, null if the studio set the status.</summary>
        public string? ApprovedBy { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        /// <summary>The songs exported in this version (many-to-many) - usually all songs of the project.</summary>
        public ICollection<Song> Songs { get; set; } = new List<Song>();

        public ICollection<MixFeedback> Feedback { get; set; } = new List<MixFeedback>();
    }
}
