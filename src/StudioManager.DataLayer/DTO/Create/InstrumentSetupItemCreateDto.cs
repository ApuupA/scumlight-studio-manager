using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// One setup entry. Exactly one of GearItemId (studio gear), ExternalGearId (band gear) or CustomName (free text).
    /// </summary>
    public class InstrumentSetupItemCreateDto
    {
        [Range(1, int.MaxValue)]
        public int ProjectInstrumentId { get; set; }

        public int? GearItemId { get; set; }
        public int? ExternalGearId { get; set; }

        [MaxLength(200)]
        public string? CustomName { get; set; }

        [MaxLength(100)]
        public string? UsedFor { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        /// <summary>Microphone or DI - needs an input channel.</summary>
        public bool NeedsInput { get; set; }

        /// <summary>0 = append at the end.</summary>
        public int SortOrder { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
