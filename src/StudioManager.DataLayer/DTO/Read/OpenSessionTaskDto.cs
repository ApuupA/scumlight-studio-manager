namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// A to-do that is not done yet, with its session and project - for the start page.
    /// </summary>
    public class OpenSessionTaskDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public int SessionId { get; set; }
        public string SessionTitle { get; set; } = string.Empty;
        public DateOnly SessionDate { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
    }
}
