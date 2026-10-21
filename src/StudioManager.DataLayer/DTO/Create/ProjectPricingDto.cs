using System.ComponentModel.DataAnnotations;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// How a project is charged: by the hour (own rate or the default rate) or with a flat price.
    /// </summary>
    public class ProjectPricingDto
    {
        public PricingMode PricingMode { get; set; } = PricingMode.Hourly;

        /// <summary>Own hourly rate of the project - null = the default rate from the settings.</summary>
        [Range(0, 10000)]
        public decimal? HourlyRate { get; set; }

        [Range(0, 10000000)]
        public decimal? FlatPrice { get; set; }

        /// <summary>Mix revisions per song included in the price, null = not agreed.</summary>
        [Range(0, 100)]
        public int? IncludedRevisions { get; set; }

        [Range(0, 100000)]
        public decimal? ExtraRevisionPrice { get; set; }
    }
}
