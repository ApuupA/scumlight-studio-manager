using System.ComponentModel.DataAnnotations;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Create
{
    public class ProjectCreateDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public ProjectType Type { get; set; } = ProjectType.Album;
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
        public DateOnly? StartDate { get; set; }
        public DateOnly? DueDate { get; set; }

        [MaxLength(4000)]
        public string? Description { get; set; }

        [Range(8000, 768000)]
        public int? SampleRate { get; set; }

        [Range(8, 64)]
        public int? BitDepth { get; set; }

        [MaxLength(200)]
        public string? Daw { get; set; }

        [Range(-60, 0)]
        public decimal? TargetLufs { get; set; }

        [MaxLength(8000)]
        public string? RecordingNotes { get; set; }

        [MaxLength(8000)]
        public string? MixingNotes { get; set; }

        [MaxLength(8000)]
        public string? MasteringNotes { get; set; }

        [Range(1, int.MaxValue)]
        public int ClientId { get; set; }

        /// <summary>
        /// Ids of the participating bands - they must belong to the client.
        /// </summary>
        public List<int> BandIds { get; set; } = new();
    }
}
