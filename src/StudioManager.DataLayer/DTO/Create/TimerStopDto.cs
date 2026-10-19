using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Stops the timer - with an optional note what was done.
    /// </summary>
    public class TimerStopDto
    {
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}
