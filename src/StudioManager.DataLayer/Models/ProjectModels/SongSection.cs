namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// One part of the song's arrangement, e.g. "Chorus 1", bar 17 - 32, "needs doubled guitars".
    /// </summary>
    public class SongSection : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>First bar of the part (optional).</summary>
        public int? FromBar { get; set; }

        /// <summary>Last bar of the part (optional).</summary>
        public int? ToBar { get; set; }

        public string? Notes { get; set; }

        /// <summary>Order within the song: 1, 2, 3, ...</summary>
        public int SortOrder { get; set; }

        public int SongId { get; set; }
        public Song? Song { get; set; }
    }
}
