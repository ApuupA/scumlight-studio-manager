using System.ComponentModel.DataAnnotations;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Create
{
    public class ProjectLinkCreateDto
    {
        public ProjectLinkType Type { get; set; } = ProjectLinkType.Reference;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, Url, MaxLength(2000)]
        public string Url { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        public int? SongId { get; set; }
    }
}
