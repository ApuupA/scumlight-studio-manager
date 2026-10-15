using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    public class GearBookingCreateDto
    {
        [Range(1, int.MaxValue)]
        public int SessionId { get; set; }

        [Range(1, int.MaxValue)]
        public int GearItemId { get; set; }

        [Range(1, 10000)]
        public int Quantity { get; set; } = 1;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
