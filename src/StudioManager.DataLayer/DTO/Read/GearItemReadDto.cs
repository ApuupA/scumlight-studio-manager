using StudioManager.DataLayer.Models.GearModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class GearItemReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public int Quantity { get; set; }
        public GearStatus Status { get; set; }
        public string? Location { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public string? Notes { get; set; }
        public bool HasPhoto { get; set; }
        public int GearCategoryId { get; set; }
        public string GearCategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Pieces booked for sessions today (calculated, not stored) - shows "in use" in the gear list.
        /// </summary>
        public int BookedTodayQuantity { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
