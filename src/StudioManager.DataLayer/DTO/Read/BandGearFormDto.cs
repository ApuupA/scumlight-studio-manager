namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// What the band sees on the public gear page (link without login): the project and the gear already listed.
    /// Only what the band needs - no client data, no studio gear.
    /// </summary>
    public class BandGearFormDto
    {
        public string ProjectTitle { get; set; } = string.Empty;

        /// <summary>The bands of the project, e.g. "Sacroscum" or "Band A, Band B".</summary>
        public string BandNames { get; set; } = string.Empty;

        public List<BandGearEntryDto> Entries { get; set; } = new();
    }

    /// <summary>
    /// One piece of band gear on the public page. IsSuggestion = entered by the band, not accepted by the studio yet.
    /// </summary>
    public class BandGearEntryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Owner { get; set; }
        public string? Notes { get; set; }
        public bool IsSuggestion { get; set; }
    }
}
