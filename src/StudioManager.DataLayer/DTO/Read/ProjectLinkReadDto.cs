using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class ProjectLinkReadDto
    {
        public int Id { get; set; }
        public ProjectLinkType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int ProjectId { get; set; }
        public int? SongId { get; set; }
        public string? SongTitle { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
