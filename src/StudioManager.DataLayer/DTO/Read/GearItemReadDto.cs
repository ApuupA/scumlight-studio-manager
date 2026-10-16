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
        public List<string> GoodFor { get; set; } = new();
        public bool HasPhoto { get; set; }
        public int GearCategoryId { get; set; }
        public string GearCategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Pieces used in today's sessions (from the setups of their instruments, calculated, not stored)
        /// - shows "in session today" in the gear list.
        /// </summary>
        public int InSessionTodayQuantity { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
