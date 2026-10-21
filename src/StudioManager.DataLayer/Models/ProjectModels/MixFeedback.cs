namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// One point of the client's feedback on a mix version, e.g. "1:23 - vocals too quiet". Ticked off when done.
    /// </summary>
    public class MixFeedback : BaseEntity
    {
        /// <summary>Position in the song as text, e.g. "1:23" - optional (general feedback has none).</summary>
        public string? Timecode { get; set; }

        public string Text { get; set; } = string.Empty;

        public bool IsDone { get; set; }

        public int MixVersionId { get; set; }
        public MixVersion? MixVersion { get; set; }
    }
}
