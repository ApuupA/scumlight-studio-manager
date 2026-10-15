namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// Equipment that is not part of the studio inventory, e.g. the band's own amps, guitars or cymbals.
    /// </summary>
    public class ExternalGear : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;

        /// <summary>Who brought it, e.g. "Lisa (drums)".</summary>
        public string? Owner { get; set; }

        public string? Notes { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }
    }
}
