namespace StudioManager.DataLayer.DTO.Read
{
    public class SessionTaskReadDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public int SortOrder { get; set; }
        public int SessionId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
