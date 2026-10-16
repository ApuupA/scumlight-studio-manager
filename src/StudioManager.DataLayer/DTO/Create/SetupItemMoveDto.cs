using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Moves a setup entry one position up (-1) or down (+1) in its chain / list.
    /// </summary>
    public class SetupItemMoveDto
    {
        [Range(-1, 1)]
        public int Direction { get; set; }
    }
}
