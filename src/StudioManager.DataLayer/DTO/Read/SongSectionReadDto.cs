namespace StudioManager.DataLayer.DTO.Read
{
    public class SongSectionReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? FromBar { get; set; }
        public int? ToBar { get; set; }
        public string? Notes { get; set; }
        public int SortOrder { get; set; }
        public int SongId { get; set; }
    }
}
