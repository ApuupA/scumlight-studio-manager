using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// Where a studio gear item is used: in which project, for which instrument - shown on the gear page.
    /// </summary>
    public class GearUsageDto
    {
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public ProjectStatus ProjectStatus { get; set; }
        public int ProjectInstrumentId { get; set; }
        public string InstrumentName { get; set; } = string.Empty;
        public string? UsedFor { get; set; }
        public int Quantity { get; set; }
    }
}
