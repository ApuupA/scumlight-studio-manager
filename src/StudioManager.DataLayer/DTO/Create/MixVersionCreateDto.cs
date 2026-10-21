using StudioManager.DataLayer.Models.ProjectModels;
using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// A mix version of a song. The number (v1, v2, ...) is given automatically.
    /// </summary>
    public class MixVersionCreateDto
    {
        [Range(1, int.MaxValue)]
        public int SongId { get; set; }

        public DateOnly Date { get; set; }

        public MixVersionStatus Status { get; set; } = MixVersionStatus.Sent;

        [Url, MaxLength(2000)]
        public string? Link { get; set; }

        [MaxLength(4000)]
        public string? Notes { get; set; }
    }
}
