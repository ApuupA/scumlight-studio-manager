namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// A standard task of the studio, e.g. "Recording / Tag" (1 day = 8 h) or "Mixing / Stunde".
    /// Time entries are booked on a standard task; the invoice shows only the standard tasks.
    /// The price comes from the hourly rate (default rate or the project's own rate) × hours per unit.
    /// </summary>
    public class StandardTask : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public TaskUnit Unit { get; set; } = TaskUnit.Hour;

        /// <summary>Hours of one unit: 1 for an hour, usually 8 for a day.</summary>
        public decimal HoursPerUnit { get; set; } = 1;

        /// <summary>Order in the lists: 1, 2, 3, ...</summary>
        public int SortOrder { get; set; }

        /// <summary>No longer offered for new entries, but old entries keep it.</summary>
        public bool IsArchived { get; set; }
    }
}
