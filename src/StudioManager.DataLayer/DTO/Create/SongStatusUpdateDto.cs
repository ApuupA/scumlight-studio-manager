using System.ComponentModel.DataAnnotations;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Sets the same status for several songs at once.
    /// </summary>
    public class SongStatusUpdateDto
    {
        [Required, MinLength(1)]
        public List<int> SongIds { get; set; } = new();

        public SongStatus Status { get; set; }
    }
}
