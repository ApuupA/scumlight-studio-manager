using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    public class ProjectInstrumentCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>True: setup is shown as signal chain (guitar -> pedal -> amp -> cab -> mic).</summary>
        public bool IsChain { get; set; }

        public int SortOrder { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        public int? BandId { get; set; }
    }
}
