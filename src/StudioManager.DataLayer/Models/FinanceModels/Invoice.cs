using StudioManager.DataLayer.Models.ClientModels;
using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// An invoice of a project. A draft can be changed freely; issuing gives it the next number, freezes
    /// the business data (SenderJson) and the totals, and locks it - corrections only via a cancellation invoice.
    /// </summary>
    public class Invoice : BaseEntity
    {
        /// <summary>Invoice number, e.g. "2026-018". Null while it is a draft.</summary>
        public string? Number { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        /// <summary>Date of the invoice (set to the issue day when issuing).</summary>
        public DateOnly InvoiceDate { get; set; }

        /// <summary>Period of the service (required on an invoice), e.g. the first and last day of the booked time.</summary>
        public DateOnly? ServiceFrom { get; set; }
        public DateOnly? ServiceTo { get; set; }

        /// <summary>Payment due date (invoice date + payment terms), set when issuing.</summary>
        public DateOnly? DueDate { get; set; }

        // ---------- Recipient (copied from the client, can be changed in the draft) ----------
        public string RecipientName { get; set; } = string.Empty;

        /// <summary>Address lines, e.g. "Max Mustermann\nMusterstr. 1\n12345 Musterstadt".</summary>
        public string? RecipientAddress { get; set; }

        // ---------- Texts ----------
        public string? IntroText { get; set; }
        public string? ClosingText { get; set; }

        // ---------- Frozen when issuing ----------

        /// <summary>The studio's business data at the moment of issuing (JSON) - later changes do not change old invoices.</summary>
        public string? SenderJson { get; set; }

        public bool IsSmallBusiness { get; set; }
        public decimal VatRate { get; set; }
        public decimal NetTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrossTotal { get; set; }

        /// <summary>The project's flat price is on this invoice (so it is not invoiced twice).</summary>
        public bool IncludesFlatPrice { get; set; }

        // ---------- Cancellation ----------

        /// <summary>For a cancellation invoice: the invoice it cancels.</summary>
        public int? CancelsInvoiceId { get; set; }
        public Invoice? CancelsInvoice { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public int ClientId { get; set; }
        public Client? Client { get; set; }

        public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
