namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// Where a setup entry comes from. Not stored - derived from which of the three fields is set.
    /// </summary>
    public enum SetupItemSource
    {
        /// <summary>Studio gear from the inventory.</summary>
        Studio,

        /// <summary>Gear the band brought (ExternalGear of the project).</summary>
        Band,

        /// <summary>Free text.</summary>
        Custom
    }
}
