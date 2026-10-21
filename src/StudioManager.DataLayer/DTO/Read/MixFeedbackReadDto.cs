namespace StudioManager.DataLayer.DTO.Read
{
    public class MixFeedbackReadDto
    {
        public int Id { get; set; }

        /// <summary>The song it is about, null for general feedback.</summary>
        public int? SongId { get; set; }
        public string? SongTitle { get; set; }

        public string? Timecode { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public int MixVersionId { get; set; }
    }
}
