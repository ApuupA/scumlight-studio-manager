namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// Business data and billing settings of the studio - exactly one row, shared by all logins.
    /// Everything that appears on an invoice besides the client and the lines.
    /// </summary>
    public class BusinessSettings : BaseEntity
    {
        // ---------- Prices ----------

        /// <summary>Hourly rate for projects without an own rate.</summary>
        public decimal DefaultHourlyRate { get; set; }

        /// <summary>Currency code, e.g. "EUR".</summary>
        public string Currency { get; set; } = "EUR";

        // ---------- Business data (sender of the invoice) ----------

        /// <summary>Name of the studio / company, e.g. "Scumlight Studio".</summary>
        public string? CompanyName { get; set; }

        /// <summary>The owner's name (required on invoices of sole proprietors).</summary>
        public string? OwnerName { get; set; }

        public string? Street { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Website { get; set; }

        /// <summary>Tax number ("Steuernummer") - an invoice needs this or the VAT ID.</summary>
        public string? TaxNumber { get; set; }

        /// <summary>VAT ID ("USt-IdNr."), optional.</summary>
        public string? VatId { get; set; }

        public string? BankName { get; set; }
        public string? Iban { get; set; }
        public string? Bic { get; set; }

        // ---------- Tax and payment ----------

        /// <summary>Small business (§ 19 UStG): invoices without VAT, with the note on the invoice.</summary>
        public bool IsSmallBusiness { get; set; } = true;

        /// <summary>VAT rate in percent, used when IsSmallBusiness is off.</summary>
        public decimal VatRate { get; set; } = 19;

        /// <summary>Days until an invoice is due.</summary>
        public int PaymentTermDays { get; set; } = 14;

        // ---------- Invoice numbers ----------

        /// <summary>Text before the number; {YYYY} is replaced by the year of the invoice, e.g. "{YYYY}-" → "2026-".</summary>
        public string InvoiceNumberPrefix { get; set; } = "{YYYY}-";

        /// <summary>The number the next invoice gets - can be set, e.g. to continue the numbers of another program.</summary>
        public int NextInvoiceNumber { get; set; } = 1;

        /// <summary>Minimum number of digits: 3 → "007".</summary>
        public int InvoiceNumberDigits { get; set; } = 3;

        // ---------- Quotes ----------

        /// <summary>Prefix of quote numbers ({YYYY} = year), e.g. "AN-{YYYY}-" → "AN-2026-001". Digits as for invoices.</summary>
        public string QuoteNumberPrefix { get; set; } = "AN-{YYYY}-";

        public int NextQuoteNumber { get; set; } = 1;

        /// <summary>How many days a quote is valid.</summary>
        public int QuoteValidityDays { get; set; } = 30;
    }
}
