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

        /// <summary>Who wrote it (user name or the name the client entered).</summary>
        public string? Author { get; set; }
        public bool IsFromClient { get; set; }

        /// <summary>From the client and not seen yet.</summary>
        public bool IsUnread { get; set; }
        public DateTime DateCreated { get; set; }
        public int MixVersionId { get; set; }
    }
}
