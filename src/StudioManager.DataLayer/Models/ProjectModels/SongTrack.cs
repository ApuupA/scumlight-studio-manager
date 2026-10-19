namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// One entry of the song's recording checklist, e.g. "Guitar L" - recorded.
    /// </summary>
    public class SongTrack : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public SongTrackStatus Status { get; set; } = SongTrackStatus.Open;

        public string? Notes { get; set; }

        /// <summary>Order within the song: 1, 2, 3, ...</summary>
        public int SortOrder { get; set; }

        public int SongId { get; set; }
        public Song? Song { get; set; }
    }
}
