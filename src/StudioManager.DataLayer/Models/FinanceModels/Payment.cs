namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// A payment received for an invoice - several for partial payments.
    /// </summary>
    public class Payment : BaseEntity
    {
        public DateOnly Date { get; set; }

        public decimal Amount { get; set; }

        public string? Note { get; set; }

        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
    }
}
