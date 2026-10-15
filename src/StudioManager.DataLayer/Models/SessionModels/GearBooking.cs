using StudioManager.DataLayer.Models.GearModels;

namespace StudioManager.DataLayer.Models.SessionModels
{
    /// <summary>
    /// Studio gear reserved for a session. For bulk items (cables, stands) the quantity says how many pieces.
    /// Double bookings are allowed - the app only warns (see GearBookingService).
    /// </summary>
    public class GearBooking : BaseEntity
    {
        public int Quantity { get; set; } = 1;
        public string? Notes { get; set; }

        public int SessionId { get; set; }
        public Session? Session { get; set; }

        public int GearItemId { get; set; }
        public GearItem? GearItem { get; set; }
    }
}
