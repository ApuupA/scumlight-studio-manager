namespace StudioManager.DataLayer.DTO.Read
{
    public class MixFeedbackReadDto
    {
        public int Id { get; set; }
        public string? Timecode { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public int MixVersionId { get; set; }
    }
}
