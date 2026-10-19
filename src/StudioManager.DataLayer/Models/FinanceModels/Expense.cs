using StudioManager.DataLayer.Models.ProjectModels;

namespace StudioManager.DataLayer.Models.FinanceModels
{
    /// <summary>
    /// A business expense with its receipt (PDF or photo), e.g. "Shure SM7B" from Thomann.
    /// Basis of the yearly overview of income and expenses.
    /// </summary>
    public class Expense : BaseEntity
    {
        /// <summary>Date of the receipt / payment.</summary>
        public DateOnly Date { get; set; }

        public string Description { get; set; } = string.Empty;

        /// <summary>Where it was bought, e.g. "Thomann".</summary>
        public string? Supplier { get; set; }

        public ExpenseCategory Category { get; set; } = ExpenseCategory.Other;

        /// <summary>The amount paid including VAT.</summary>
        public decimal GrossAmount { get; set; }

        /// <summary>VAT rate on the receipt in percent (19, 7 or 0).</summary>
        public decimal VatRate { get; set; } = 19;

        public string? Notes { get; set; }

        /// <summary>Optional: the project the expense belongs to.</summary>
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        /// <summary>Relative path of the receipt file in the file storage, null if there is none.</summary>
        public string? ReceiptPath { get; set; }

        /// <summary>Original file name of the receipt, e.g. "Rechnung_Thomann.pdf".</summary>
        public string? ReceiptFileName { get; set; }
    }
}
