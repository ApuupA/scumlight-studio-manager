namespace StudioManager.DataLayer.DTO.Read
{
    public class ExternalGearReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Owner { get; set; }
        public string? Notes { get; set; }

        /// <summary>Entered by the band via the link, not accepted yet.</summary>
        public bool IsSuggestion { get; set; }
        public int ProjectId { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
