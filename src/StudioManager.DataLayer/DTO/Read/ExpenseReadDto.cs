using StudioManager.DataLayer.Models.FinanceModels;

namespace StudioManager.DataLayer.DTO.Read
{
    public class ExpenseReadDto
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Supplier { get; set; }
        public ExpenseCategory Category { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal VatRate { get; set; }

        /// <summary>Gross amount without VAT.</summary>
        public decimal NetAmount { get; set; }

        /// <summary>The VAT contained in the gross amount.</summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Equipment or software over 800 EUR net: may have to be depreciated over several years (AfA)
        /// instead of being an expense of one year.
        /// </summary>
        public bool IsAboveDepreciationLimit { get; set; }

        public string? Notes { get; set; }
        public int? ProjectId { get; set; }
        public string? ProjectTitle { get; set; }

        public bool HasReceipt { get; set; }
        public string? ReceiptFileName { get; set; }

        /// <summary>Changes when the receipt is replaced, so the browser loads the new file.</summary>
        public DateTime DateUpdated { get; set; }
    }
}
