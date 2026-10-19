namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// One line of an invoice, e.g. "Recording / Tag" 3 Tage × 300,00 €.
    /// </summary>
    public class InvoiceLine : BaseEntity
    {
        public string Description { get; set; } = string.Empty;

        public decimal Quantity { get; set; }

        /// <summary>Unit as text, e.g. "Tage", "Std.", "Stk.", "pauschal".</summary>
        public string? Unit { get; set; }

        public decimal UnitPrice { get; set; }

        /// <summary>Quantity × unit price, rounded to cents.</summary>
        public decimal Total { get; set; }

        public int SortOrder { get; set; }

        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
    }
}
