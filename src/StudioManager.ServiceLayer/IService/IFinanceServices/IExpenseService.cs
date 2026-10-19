using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;
using StudioManager.DataLayer.Models.FinanceModels;
using StudioManager.ServiceLayer.Results;

namespace StudioManager.ServiceLayer.IService.IFinanceServices
{
    /// <summary>
    /// Expenses with receipts and the yearly overview of income and expenses (basis for the EÜR).
    /// </summary>
    public interface IExpenseService : IBaseService<Expense, ExpenseReadDto, ExpenseCreateDto>
    {
        /// <summary>The expenses of one year, newest first.</summary>
        Task<IEnumerable<ExpenseReadDto>> GetForYear(int year);

        /// <summary>
        /// Saves the receipt (PDF, JPG, PNG or WEBP, max. 10 MB) and deletes the old one.
        /// </summary>
        Task<ExpenseReadDto> SetReceipt(int id, Stream content, string fileName, long length);

        /// <summary>The receipt file, null if there is none.</summary>
        Task<FileDownload?> GetReceipt(int id);

        Task<ExpenseReadDto> DeleteReceipt(int id);

        /// <summary>
        /// Payments received and expenses of one year: totals per month and per category, and all bookings.
        /// </summary>
        Task<YearOverviewDto> GetYearOverview(int year);
    }
}
