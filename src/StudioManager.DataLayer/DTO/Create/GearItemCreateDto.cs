using System.ComponentModel.DataAnnotations;
using StudioManager.DataLayer.Models.GearModels;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Input for creating (POST) and updating (PUT) a gear item.
    /// The validation attributes are checked automatically by [ApiController] - invalid input returns 400.
    /// </summary>
    public class GearItemCreateDto
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Manufacturer { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        [Range(1, 10000)]
        public int Quantity { get; set; } = 1;

        public GearStatus Status { get; set; } = GearStatus.Available;

        [MaxLength(100)]
        public string? Location { get; set; }

        public DateOnly? PurchaseDate { get; set; }

        [Range(0, 10_000_000)]
        public decimal? PurchasePrice { get; set; }

        [MaxLength(4000)]
        public string? Notes { get; set; }

        [Range(1, int.MaxValue)]
        public int GearCategoryId { get; set; }
    }
}
