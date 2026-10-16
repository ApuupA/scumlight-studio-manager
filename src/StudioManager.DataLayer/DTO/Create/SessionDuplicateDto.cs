using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Dates to copy a session to, e.g. the next four days for a series of drum recording days.
    /// </summary>
    public class SessionDuplicateDto
    {
        [Required, MinLength(1), MaxLength(60)]
        public List<DateOnly> Dates { get; set; } = new();
    }
}
