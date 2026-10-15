namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// A link to a reference track, rough mix, demo etc. (e.g. Spotify, YouTube, Dropbox).
    /// </summary>
    public class ProjectLink : BaseEntity
    {
        public ProjectLinkType Type { get; set; } = ProjectLinkType.Reference;
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        /// <summary>
        /// Optional: the song this link belongs to (e.g. the rough mix of song 3).
        /// </summary>
        public int? SongId { get; set; }
        public Song? Song { get; set; }
    }
}
