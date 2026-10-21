using StudioManager.DataLayer.Models.ProjectModels;
using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// A mix version of the project. The order number is given automatically.
    /// </summary>
    public class MixVersionCreateDto
    {
        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        /// <summary>True = a demo of the songwriting. Set when creating - a version stays a mix or a demo.</summary>
        public bool IsDemo { get; set; }

        /// <summary>e.g. "v1.1" - empty = "v" + the next number ("Demo " + number for demos).</summary>
        [MaxLength(30)]
        public string? Name { get; set; }

        public DateOnly Date { get; set; }

        public MixVersionStatus Status { get; set; } = MixVersionStatus.Sent;

        [Url, MaxLength(2000)]
        public string? Link { get; set; }

        [MaxLength(4000)]
        public string? Notes { get; set; }

        /// <summary>The exported songs - at least one, all of them belong to the project.</summary>
        [MinLength(1)]
        public List<int> SongIds { get; set; } = new();
    }
}
