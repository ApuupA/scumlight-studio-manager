using StudioManager.DataLayer.Models.GearModels;

namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// One piece of the setup of an instrument, e.g. "SM57 - Snare Top" or "JCM800" in a guitar chain.
    /// Exactly one source is set: studio gear (GearItem), gear the band brought (ExternalGear) or free text (CustomName).
    /// </summary>
    public class InstrumentSetupItem : BaseEntity
    {
        public int? GearItemId { get; set; }
        public GearItem? GearItem { get; set; }

        public int? ExternalGearId { get; set; }
        public ExternalGear? ExternalGear { get; set; }

        /// <summary>Free text for anything not in the lists, e.g. "Rented Fender Twin".</summary>
        public string? CustomName { get; set; }

        /// <summary>What it is used for, e.g. "Snare Top", "Kick In", "Cab".</summary>
        public string? UsedFor { get; set; }

        public int Quantity { get; set; } = 1;

        /// <summary>
        /// True for microphones and DIs - every such entry needs an input channel (used for the input lists).
        /// </summary>
        public bool NeedsInput { get; set; }

        /// <summary>Position in the setup / signal chain (ascending).</summary>
        public int SortOrder { get; set; }

        public string? Notes { get; set; }

        public int ProjectInstrumentId { get; set; }
        public ProjectInstrument? ProjectInstrument { get; set; }
    }
}
