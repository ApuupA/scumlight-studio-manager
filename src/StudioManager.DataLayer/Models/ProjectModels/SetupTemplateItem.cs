using StudioManager.DataLayer.Models.GearModels;

namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// One entry of a setup template - same fields as InstrumentSetupItem, without band gear.
    /// </summary>
    public class SetupTemplateItem : BaseEntity
    {
        public int? GearItemId { get; set; }
        public GearItem? GearItem { get; set; }

        public string? CustomName { get; set; }
        public string? UsedFor { get; set; }
        public int Quantity { get; set; } = 1;
        public bool NeedsInput { get; set; }
        public int SortOrder { get; set; }
        public string? Notes { get; set; }

        public int SetupTemplateId { get; set; }
        public SetupTemplate? SetupTemplate { get; set; }
    }
}
