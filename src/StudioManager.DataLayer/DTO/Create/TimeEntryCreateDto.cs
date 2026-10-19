using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Working time on a project, optionally of one of its sessions.
    /// </summary>
    public class TimeEntryCreateDto
    {
        public DateOnly Date { get; set; }

        [Range(0.01, 24)]
        public decimal Hours { get; set; }

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsBillable { get; set; } = true;

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        public int? SessionId { get; set; }

        /// <summary>Who worked - usually the logged in user.</summary>
        public int? UserId { get; set; }
    }
}
