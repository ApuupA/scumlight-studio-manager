namespace StudioManager.DataLayer.DTO.Read
{
    public class DrumTuningReadDto
    {
        public int Id { get; set; }
        public string Drum { get; set; } = string.Empty;
        public decimal? BatterHz { get; set; }
        public decimal? ResonantHz { get; set; }
        public string? Heads { get; set; }
        public string? Notes { get; set; }
        public int ProjectId { get; set; }
        public int? SongId { get; set; }
        public string? SongTitle { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
