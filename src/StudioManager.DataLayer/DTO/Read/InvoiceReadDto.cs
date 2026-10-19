using StudioManager.DataLayer.Models.FinanceModels;

namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// An invoice with lines, payments and everything needed to print it.
    /// </summary>
    public class InvoiceReadDto
    {
        public int Id { get; set; }
        public string? Number { get; set; }
        public InvoiceStatus Status { get; set; }
        public InvoicePaymentStatus PaymentStatus { get; set; }

        public DateOnly InvoiceDate { get; set; }
        public DateOnly? ServiceFrom { get; set; }
        public DateOnly? ServiceTo { get; set; }
        public DateOnly? DueDate { get; set; }

        public string RecipientName { get; set; } = string.Empty;
        public string? RecipientAddress { get; set; }
        public string? IntroText { get; set; }
        public string? ClosingText { get; set; }

        public bool IsSmallBusiness { get; set; }
        public decimal VatRate { get; set; }
        public decimal NetTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OpenAmount { get; set; }

        public int? CancelsInvoiceId { get; set; }
        public string? CancelsInvoiceNumber { get; set; }

        /// <summary>For a cancelled invoice: the number of the cancellation invoice.</summary>
        public string? CancelledByNumber { get; set; }

        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;

        /// <summary>The sender: frozen at issuing, the current business data for a draft.</summary>
        public BusinessSettingsReadDto Sender { get; set; } = new();

        public List<InvoiceLineDto> Lines { get; set; } = new();
        public List<PaymentReadDto> Payments { get; set; } = new();
    }
}
