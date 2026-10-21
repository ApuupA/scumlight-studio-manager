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

        /// <summary>For sorting: the newest projects first ("what did we use on the last record?").</summary>
        public DateOnly? ProjectStartDate { get; set; }
        public int ProjectInstrumentId { get; set; }
        public string InstrumentName { get; set; } = string.Empty;
        public string? UsedFor { get; set; }
        public int Quantity { get; set; }

        /// <summary>The notes of the setup entry, e.g. the settings: "NAM model Plexi Crunch, gain 6".</summary>
        public string? Notes { get; set; }
    }
}
