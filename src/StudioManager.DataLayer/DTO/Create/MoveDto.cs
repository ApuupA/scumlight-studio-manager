using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Moves an entry of a list one position up (-1) or down (+1).
    /// </summary>
    public class MoveDto
    {
        [Range(-1, 1)]
        public int Direction { get; set; }
    }
}
