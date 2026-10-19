namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// One tempo of a song. A song with tempo changes has several, e.g. 190 BPM, then 120 BPM from bar 121.
    /// </summary>
    public class SongTempo : BaseEntity
    {
        public decimal Bpm { get; set; }

        /// <summary>Bar where this tempo starts (optional).</summary>
        public int? FromBar { get; set; }

        /// <summary>Order within the song: 1, 2, 3, ...</summary>
        public int SortOrder { get; set; }

        public int SongId { get; set; }
        public Song? Song { get; set; }
    }
}
