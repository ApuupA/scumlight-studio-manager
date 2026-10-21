namespace StudioManager.DataLayer.DTO.Read
{
    public class ClientReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Notes { get; set; }
        public int BandCount { get; set; }
        public int ProjectCount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        /// <summary>True if a picture was uploaded (GET .../{id}/photo).</summary>
        public bool HasPhoto { get; set; }

        /// <summary>
        /// Without an own picture: the picture of the client's only band (most clients are exactly one band),
        /// so it does not have to be uploaded twice. Null if there is none.
        /// </summary>
        public PhotoRefDto? FallbackPhoto { get; set; }
    }
}
