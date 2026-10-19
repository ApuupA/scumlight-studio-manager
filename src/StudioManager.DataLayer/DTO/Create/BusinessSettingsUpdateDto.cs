using System.ComponentModel.DataAnnotations;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// Business data and billing settings of the studio (profile page) - shared by all logins.
    /// </summary>
    public class BusinessSettingsUpdateDto
    {
        [Range(0, 10000)]
        public decimal DefaultHourlyRate { get; set; }

        [MaxLength(200)] public string? CompanyName { get; set; }
        [MaxLength(200)] public string? OwnerName { get; set; }
        [MaxLength(200)] public string? Street { get; set; }
        [MaxLength(20)] public string? PostalCode { get; set; }
        [MaxLength(100)] public string? City { get; set; }
        [MaxLength(200), EmailAddress] public string? Email { get; set; }
        [MaxLength(50)] public string? Phone { get; set; }
        [MaxLength(200)] public string? Website { get; set; }
        [MaxLength(50)] public string? TaxNumber { get; set; }
        [MaxLength(50)] public string? VatId { get; set; }
        [MaxLength(100)] public string? BankName { get; set; }
        [MaxLength(50)] public string? Iban { get; set; }
        [MaxLength(20)] public string? Bic { get; set; }

        public bool IsSmallBusiness { get; set; } = true;

        [Range(0, 100)]
        public decimal VatRate { get; set; } = 19;

        [Range(0, 365)]
        public int PaymentTermDays { get; set; } = 14;

        /// <summary>Text before the number, {YYYY} = year of the invoice, e.g. "{YYYY}-" or "RE-".</summary>
        [MaxLength(30)]
        public string InvoiceNumberPrefix { get; set; } = "{YYYY}-";

        [Range(1, 999999)]
        public int NextInvoiceNumber { get; set; } = 1;

        [Range(1, 8)]
        public int InvoiceNumberDigits { get; set; } = 3;
    }
}
