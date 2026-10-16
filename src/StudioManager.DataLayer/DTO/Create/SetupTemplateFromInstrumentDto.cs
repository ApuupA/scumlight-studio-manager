using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Saves the setup of an instrument as reusable template (band gear is left out).
    /// </summary>
    public class SetupTemplateFromInstrumentDto
    {
        [Range(1, int.MaxValue)]
        public int ProjectInstrumentId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
