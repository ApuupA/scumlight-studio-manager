using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Starts the timer of the logged in user for a project.
    /// </summary>
    public class TimerStartDto
    {
        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        public int? SessionId { get; set; }

        /// <summary>What is being done - can also be filled in when stopping.</summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>Today in the browser's time zone (the server may run in UTC).</summary>
        public DateOnly Date { get; set; }
    }
}
