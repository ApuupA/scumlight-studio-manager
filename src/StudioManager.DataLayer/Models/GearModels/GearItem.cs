namespace StudioManager.DataLayer.Models.GearModels
{
    public class GearItem : BaseEntity, IHasPhoto
    {
        public string Name { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }

        /// <summary>
        /// For bulk items like XLR cables or stands. Unique pieces (e.g. a specific U87) have quantity 1.
        /// </summary>
        public int Quantity { get; set; } = 1;

        public GearStatus Status { get; set; } = GearStatus.Available;
        public string? Location { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public string? Notes { get; set; }

        /// <summary>
        /// What the gear is good for, as short tags, e.g. ["Snare", "Guitar cab"]. Helps to find the right gear.
        /// Stored as a text array in PostgreSQL.
        /// </summary>
        public List<string> GoodFor { get; set; } = new();

        /// <summary>
        /// Path of the photo relative to the file storage root, null if no photo was uploaded.
        /// </summary>
        public string? PhotoPath { get; set; }

        public int GearCategoryId { get; set; }
        public GearCategory? GearCategory { get; set; }
    }
}
