namespace StudioManager.DataLayer.Models.ProjectModels
{
    /// <summary>
    /// How a project is charged.
    /// </summary>
    public enum PricingMode
    {
        /// <summary>Billable hours × hourly rate (the project's own rate or the default rate).</summary>
        Hourly,

        /// <summary>An agreed total price, independent of the hours.</summary>
        FlatPrice
    }
}
