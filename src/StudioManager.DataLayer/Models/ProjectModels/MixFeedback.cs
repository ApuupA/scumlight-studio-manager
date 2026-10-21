namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// One point of the client's feedback on a mix version, e.g. song 3 at "1:23" - "vocals too quiet".
    /// Ticked off when done.
    /// </summary>
    public class MixFeedback : BaseEntity
    {
        /// <summary>The song it is about - null for general feedback ("more low end on all songs").</summary>
        public int? SongId { get; set; }
        public Song? Song { get; set; }

        /// <summary>Position in the song as text, e.g. "1:23" - optional.</summary>
        public string? Timecode { get; set; }

        public string Text { get; set; } = string.Empty;

        public bool IsDone { get; set; }

        /// <summary>Who wrote it: the name of the logged in user, or the name the client entered on the client page.</summary>
        public string? Author { get; set; }

        /// <summary>Written on the client page (link without login).</summary>
        public bool IsFromClient { get; set; }

        /// <summary>From the client and not seen by the studio yet - shown as "new" (start page, tab "Mixing").</summary>
        public bool IsUnread { get; set; }

        public int MixVersionId { get; set; }
        public MixVersion? MixVersion { get; set; }
    }
}
