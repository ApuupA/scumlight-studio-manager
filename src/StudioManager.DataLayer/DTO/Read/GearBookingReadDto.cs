using StudioManager.DataLayer.Models.GearModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class GearBookingReadDto
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }

        public int SessionId { get; set; }
        public SessionShortDto Session { get; set; } = new();

        // ---------- The booked gear item ----------
        public int GearItemId { get; set; }
        public string GearItemName { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string GearCategoryName { get; set; } = string.Empty;
        public GearStatus GearStatus { get; set; }

        /// <summary>How many pieces the studio owns (GearItem.Quantity).</summary>
        public int AvailableQuantity { get; set; }

        // ---------- Warnings (calculated by GearBookingService) ----------
        /// <summary>Pieces booked at the same time: this booking plus all overlapping sessions.</summary>
        public int TotalBookedQuantity { get; set; }

        /// <summary>True if more pieces are booked at the same time than the studio owns.</summary>
        public bool IsOverbooked { get; set; }

        /// <summary>Other sessions that book the same gear at an overlapping time.</summary>
        public List<SessionShortDto> ConflictingSessions { get; set; } = new();
    }
}
