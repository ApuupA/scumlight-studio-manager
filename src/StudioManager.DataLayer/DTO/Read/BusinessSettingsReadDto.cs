namespace StudioManager.DataLayer.DTO.Read
{
    public class BusinessSettingsReadDto
    {
        public decimal DefaultHourlyRate { get; set; }
        public string Currency { get; set; } = "EUR";

        public string? CompanyName { get; set; }
        public string? OwnerName { get; set; }
        public string? Street { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }
        public string? TaxNumber { get; set; }
        public string? VatId { get; set; }
        public string? BankName { get; set; }
        public string? Iban { get; set; }
        public string? Bic { get; set; }

        public bool IsSmallBusiness { get; set; }
        public decimal VatRate { get; set; }
        public int PaymentTermDays { get; set; }

        public string InvoiceNumberPrefix { get; set; } = string.Empty;
        public int NextInvoiceNumber { get; set; }
        public int InvoiceNumberDigits { get; set; }

        /// <summary>How the next invoice number will look this year, e.g. "2026-018".</summary>
        public string NextInvoiceNumberPreview { get; set; } = string.Empty;

        public string QuoteNumberPrefix { get; set; } = string.Empty;
        public int NextQuoteNumber { get; set; }
        public int QuoteValidityDays { get; set; }

        /// <summary>How the next quote number will look this year, e.g. "AN-2026-001".</summary>
        public string NextQuoteNumberPreview { get; set; } = string.Empty;
    }
}
