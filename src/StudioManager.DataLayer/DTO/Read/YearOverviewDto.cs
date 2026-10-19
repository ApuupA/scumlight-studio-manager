using StudioManager.DataLayer.Models.FinanceModels;

namespace StudioManager.DataLayer.DTO.Read
{
    /// <summary>
    /// Income (payments received) and expenses of one year - the basis for the EÜR.
    /// All amounts are gross; the VAT parts are shown separately for studios that charge VAT.
    /// </summary>
    public class YearOverviewDto
    {
        public int Year { get; set; }

        /// <summary>Small business (§ 19 UStG) according to the current settings.</summary>
        public bool IsSmallBusiness { get; set; }

        public decimal IncomeTotal { get; set; }
        public decimal ExpensesTotal { get; set; }

        /// <summary>Income minus expenses.</summary>
        public decimal Result { get; set; }

        /// <summary>VAT contained in the income (only with VAT invoices).</summary>
        public decimal IncomeVat { get; set; }

        /// <summary>VAT contained in the expenses (input tax).</summary>
        public decimal ExpensesVat { get; set; }

        /// <summary>Always 12 entries, January to December.</summary>
        public List<MonthTotalDto> Months { get; set; } = [];

        /// <summary>Only the categories with expenses, largest first.</summary>
        public List<CategoryTotalDto> Categories { get; set; } = [];

        /// <summary>All payments and expenses of the year by date - for the list and the CSV export.</summary>
        public List<BookingDto> Bookings { get; set; } = [];
    }

    public class MonthTotalDto
    {
        /// <summary>1 = January.</summary>
        public int Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
    }

    public class CategoryTotalDto
    {
        public ExpenseCategory Category { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// One line of the yearly list: a payment received or an expense.
    /// </summary>
    public class BookingDto
    {
        public DateOnly Date { get; set; }

        /// <summary>True for a payment received, false for an expense.</summary>
        public bool IsIncome { get; set; }

        /// <summary>The invoice id (income) or expense id - for the link in the list.</summary>
        public int SourceId { get; set; }

        /// <summary>Invoice number for income, null for expenses.</summary>
        public string? InvoiceNumber { get; set; }

        public string Description { get; set; } = string.Empty;

        /// <summary>Client (income) or supplier (expense).</summary>
        public string? Party { get; set; }

        /// <summary>Only for expenses.</summary>
        public ExpenseCategory? Category { get; set; }

        public string? ProjectTitle { get; set; }
        public decimal Net { get; set; }
        public decimal Vat { get; set; }
        public decimal Gross { get; set; }
        public bool HasReceipt { get; set; }
    }
}
