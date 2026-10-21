namespace StudioManager.DataLayer.DTO.Read
{
    public class VoiceMemoReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateOnly? RecordedOn { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int ProjectId { get; set; }
        public int? SongId { get; set; }
        public string? SongTitle { get; set; }
        public int? SongSectionId { get; set; }
        public string? SongSectionName { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
