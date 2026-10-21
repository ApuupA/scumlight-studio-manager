namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// One mix of a song (v1, v2, ...) with the client's feedback. Every version after the first is a revision.
    /// </summary>
    public class MixVersion : BaseEntity
    {
        /// <summary>1, 2, 3, ... per song - given automatically.</summary>
        public int Number { get; set; }

        public DateOnly Date { get; set; }

        public MixVersionStatus Status { get; set; } = MixVersionStatus.Sent;

        /// <summary>Where the bounce is, e.g. a Nextcloud share link.</summary>
        public string? Link { get; set; }

        /// <summary>What changed in this version, e.g. "Vocals +1 dB, less reverb on the snare".</summary>
        public string? Notes { get; set; }

        public int SongId { get; set; }
        public Song? Song { get; set; }

        public ICollection<MixFeedback> Feedback { get; set; } = new List<MixFeedback>();
    }
}
