using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// A quote ("Angebot") for a project. Like an invoice: a draft can be changed, issuing gives it a number
    /// (own number range) and freezes it. An accepted quote can become an invoice.
    /// </summary>
    public class Quote : BaseEntity
    {
        /// <summary>Quote number, e.g. "AN-2026-001". Null while it is a draft.</summary>
        public string? Number { get; set; }

        public QuoteStatus Status { get; set; } = QuoteStatus.Draft;

        public DateOnly QuoteDate { get; set; }

        /// <summary>How long the offer is valid (set when issuing: date + validity days).</summary>
        public DateOnly? ValidUntil { get; set; }

        public string RecipientName { get; set; } = string.Empty;
        public string? RecipientAddress { get; set; }
        public string? IntroText { get; set; }
        public string? ClosingText { get; set; }

        // ---------- Frozen when issuing ----------
        public string? SenderJson { get; set; }
        public bool IsSmallBusiness { get; set; }
        public decimal VatRate { get; set; }
        public decimal NetTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrossTotal { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public int ClientId { get; set; }
        public Client? Client { get; set; }

        /// <summary>The invoice made from this quote (status Invoiced).</summary>
        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        public ICollection<QuoteLine> Lines { get; set; } = new List<QuoteLine>();
    }
}
