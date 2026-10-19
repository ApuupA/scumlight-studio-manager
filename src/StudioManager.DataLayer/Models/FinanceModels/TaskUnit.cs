namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// How a standard task is counted on the invoice.
    /// </summary>
    public enum TaskUnit
    {
        /// <summary>Per hour, e.g. "Mixing / Stunde".</summary>
        Hour,

        /// <summary>Per day with a fixed number of hours, e.g. "Recording / Tag" = 8 h.</summary>
        Day
    }
}
