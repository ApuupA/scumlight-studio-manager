using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// The material for the mix, once per project (usually all songs with one link). Tab "Mixing".
    /// </summary>
    public class ProjectMaterialDto
    {
        public DateOnly? MaterialReceivedOn { get; set; }

        /// <summary>Where the multitracks / stems are, e.g. a WeTransfer or Nextcloud link.</summary>
        [Url, MaxLength(2000)]
        public string? MaterialLink { get; set; }

        [Range(0, 10000)]
        public int? MaterialTrackCount { get; set; }

        [MaxLength(2000)]
        public string? MaterialNotes { get; set; }
    }
}
