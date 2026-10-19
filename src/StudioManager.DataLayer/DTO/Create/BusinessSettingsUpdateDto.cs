using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Billing settings of the studio (settings page).
    /// </summary>
    public class BusinessSettingsUpdateDto
    {
        [Range(0, 10000)]
        public decimal DefaultHourlyRate { get; set; }
    }
}
