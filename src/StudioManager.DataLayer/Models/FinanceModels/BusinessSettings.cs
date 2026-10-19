namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// Billing settings of the studio - exactly one row. Invoices (business data, small business / VAT)
    /// are added here in the next finance step.
    /// </summary>
    public class BusinessSettings : BaseEntity
    {
        /// <summary>Hourly rate for projects without an own rate.</summary>
        public decimal DefaultHourlyRate { get; set; }

        /// <summary>Currency code, e.g. "EUR".</summary>
        public string Currency { get; set; } = "EUR";
    }
}
