using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// An entry of the deliverables checklist of a project, e.g. "DDP (CD-Presswerk)".
    /// </summary>
    public class ProjectDeliverableCreateDto
    {
        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public bool IsDone { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public int SortOrder { get; set; }
    }
}
