using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// Hours and value of a project ("what has it cost so far?").
    /// </summary>
    public class ProjectFinanceDto
    {
        public PricingMode PricingMode { get; set; }

        /// <summary>The project's own rate, null = default rate.</summary>
        public decimal? HourlyRate { get; set; }

        /// <summary>The rate used for the calculation (own rate or default rate).</summary>
        public decimal EffectiveHourlyRate { get; set; }

        public decimal? FlatPrice { get; set; }

        public decimal TotalHours { get; set; }
        public decimal BillableHours { get; set; }

        /// <summary>Hourly: billable hours × rate. Flat price: the flat price.</summary>
        public decimal LaborValue { get; set; }

        /// <summary>Sum of the billable extra items.</summary>
        public decimal ExtrasValue { get; set; }

        public decimal TotalValue { get; set; }

        /// <summary>Flat price only: what one worked hour actually earns (flat price / all hours). Null without hours.</summary>
        public decimal? ActualHourlyRate { get; set; }

        /// <summary>Sum of the issued invoices (gross, cancellations subtracted).</summary>
        public decimal InvoicedTotal { get; set; }

        /// <summary>Sum of the payments received.</summary>
        public decimal PaidTotal { get; set; }

        public string Currency { get; set; } = "EUR";
    }
}
