using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class SongTrackReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SongTrackStatus Status { get; set; }
        public string? Notes { get; set; }
        public int SortOrder { get; set; }
        public int SongId { get; set; }
    }
}
