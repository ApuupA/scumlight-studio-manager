using StudioManager.DataLayer.Models.GearModels;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class InstrumentSetupItemReadDto
    {
        public int Id { get; set; }
        public int ProjectInstrumentId { get; set; }

        /// <summary>Studio gear, band gear or free text.</summary>
        public SetupItemSource Source { get; set; }

        /// <summary>The name to show, whatever the source: gear name, band gear name or the free text.</summary>
        public string DisplayName { get; set; } = string.Empty;

        public int? GearItemId { get; set; }
        public string? GearCategoryName { get; set; }

        /// <summary>Status of the studio gear - the frontend warns if it is not "Available".</summary>
        public GearStatus? GearStatus { get; set; }

        public int? ExternalGearId { get; set; }

        /// <summary>Who brought the band gear.</summary>
        public string? ExternalGearOwner { get; set; }

        public string? CustomName { get; set; }
        public string? UsedFor { get; set; }
        public int Quantity { get; set; }
        public bool NeedsInput { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }
    }
}
