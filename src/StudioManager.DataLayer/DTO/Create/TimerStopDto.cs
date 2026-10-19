using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Stops the timer - with the description of what was done.
    /// </summary>
    public class TimerStopDto
    {
        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}
