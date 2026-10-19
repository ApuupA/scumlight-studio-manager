using StudioManager.DataLayer.Models.FinanceModels;

namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// A quote with lines and everything needed to print it.
    /// </summary>
    public class QuoteReadDto
    {
        public int Id { get; set; }
        public string? Number { get; set; }
        public QuoteStatus Status { get; set; }

        /// <summary>Sent, not answered and past ValidUntil.</summary>
        public bool IsExpired { get; set; }

        public DateOnly QuoteDate { get; set; }
        public DateOnly? ValidUntil { get; set; }

        public string RecipientName { get; set; } = string.Empty;
        public string? RecipientAddress { get; set; }
        public string? IntroText { get; set; }
        public string? ClosingText { get; set; }

        public bool IsSmallBusiness { get; set; }
        public decimal VatRate { get; set; }
        public decimal NetTotal { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrossTotal { get; set; }

        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;

        public int? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }

        /// <summary>The sender: frozen at issuing, the current business data for a draft.</summary>
        public BusinessSettingsReadDto Sender { get; set; } = new();

        public List<InvoiceLineDto> Lines { get; set; } = new();
    }
}
