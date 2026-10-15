namespace StudioManager.DataLayer.Models.SessionModels
{
    /// <summary>
    /// One item of the session's to-do list, e.g. "Drums song 1-4" or "Reamp guitars".
    /// </summary>
    public class SessionTask : BaseEntity
    {
        public string Text { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public int SortOrder { get; set; }

        public int SessionId { get; set; }
        public Session? Session { get; set; }
    }
}
