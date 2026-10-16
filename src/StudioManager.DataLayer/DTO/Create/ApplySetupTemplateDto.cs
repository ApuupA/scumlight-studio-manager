using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Creates a new instrument in a project from a setup template.
    /// </summary>
    public class ApplySetupTemplateDto
    {
        [Range(1, int.MaxValue)]
        public int ProjectId { get; set; }

        [Range(1, int.MaxValue)]
        public int SetupTemplateId { get; set; }

        /// <summary>Name of the new instrument, e.g. "Drums". Empty = name of the template.</summary>
        [MaxLength(100)]
        public string? InstrumentName { get; set; }

        public int? BandId { get; set; }
    }
}
