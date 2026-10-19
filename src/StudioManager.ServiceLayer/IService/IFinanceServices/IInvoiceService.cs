using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;

namespace StudioManager.ServiceLayer.IService.IFinanceServices
{
    /// <summary>
    /// Invoices: draft from the open hours and extra items → issue (number, locked) → payments; corrections by cancellation.
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>All invoices, newest first.</summary>
        Task<IEnumerable<InvoiceReadDto>> GetAll();

        Task<IEnumerable<InvoiceReadDto>> GetForProject(int projectId);

        Task<InvoiceReadDto?> Get(int id);

        /// <summary>
        /// A new draft with everything of the project that was not invoiced yet: billable hours grouped by
        /// standard task, billable extra items, the flat price (once). The included entries are reserved for it.
        /// </summary>
        Task<InvoiceReadDto> CreateDraft(int projectId);

        /// <summary>Changes a draft (recipient, dates, texts, lines).</summary>
        Task<InvoiceReadDto> UpdateDraft(int id, InvoiceUpdateDto dto);

        /// <summary>Deletes a draft - its hours and extra items are open again. Issued invoices can not be deleted.</summary>
        Task DeleteDraft(int id);

        /// <summary>Issues a draft: next number, business data frozen, due date, locked.</summary>
        Task<InvoiceReadDto> Issue(int id, InvoiceDateDto dto);

        /// <summary>
        /// Cancels an issued invoice with a cancellation invoice (negative lines, own number).
        /// Its hours and extra items are open again. Returns the cancellation invoice.
        /// </summary>
        Task<InvoiceReadDto> Cancel(int id, InvoiceDateDto dto);

        Task<InvoiceReadDto> AddPayment(int invoiceId, PaymentCreateDto dto);

        Task<InvoiceReadDto> DeletePayment(int invoiceId, int paymentId);
    }
}
