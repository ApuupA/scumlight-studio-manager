using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// A piece of gear the band enters on the public gear page. It becomes a suggestion the studio accepts or rejects.
    /// </summary>
    public class BandGearSuggestionDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        /// <summary>Who brings it, e.g. "Max (guitar)".</summary>
        [MaxLength(200)]
        public string? Owner { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
