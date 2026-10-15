using System.ComponentModel.DataAnnotations;
using StudioManager.DataLayer.Models.SessionModels;

namespace StudioManager.DataLayer.DTO.Create
{
    public class SessionCreateDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public SessionType Type { get; set; } = SessionType.Recording;
        public SessionStatus Status { get; set; } = SessionStatus.Planned;

        public DateOnly Date { get; set; }

        /// <summary>
        /// Both null = all-day session. Otherwise both must be set and EndTime must be after StartTime.
        /// </summary>
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        [MaxLength(200)]
        public string? Engineer { get; set; }

        [MaxLength(500)]
        public string? Attendees { get; set; }

        [MaxLength(4000)]
        public string? Goals { get; set; }

        [MaxLength(4000)]
        public string? Summary { get; set; }

        [MaxLength(4000)]
        public string? NextSteps { get; set; }

        [MaxLength(4000)]
        public string? Notes { get; set; }

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        /// <summary>
        /// Songs worked on in this session - they must belong to the project.
        /// </summary>
        public List<int> SongIds { get; set; } = new();
    }
}
