namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// One line of a quote, e.g. "Recording / Tag" 5 Tage × 300,00 €.
    /// </summary>
    public class QuoteLine : BaseEntity
    {
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal UnitPrice { get; set; }

        /// <summary>Quantity × unit price, rounded to cents.</summary>
        public decimal Total { get; set; }

        public int SortOrder { get; set; }

        public int QuoteId { get; set; }
        public Quote? Quote { get; set; }
    }
}
