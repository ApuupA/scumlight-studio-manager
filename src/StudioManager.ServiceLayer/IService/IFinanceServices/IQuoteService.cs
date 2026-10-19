using StudioManager.DataLayer.DTO.Create;
using StudioManager.DataLayer.DTO.Read;

namespace StudioManager.ServiceLayer.IService.IFinanceServices
{
    /// <summary>
    /// Quotes: draft → issue (number, sent) → accepted / declined → invoice (InvoiceService.CreateDraftFromQuote).
    /// </summary>
    public interface IQuoteService
    {
        /// <summary>All quotes, newest first.</summary>
        Task<IEnumerable<QuoteReadDto>> GetAll();

        Task<IEnumerable<QuoteReadDto>> GetForProject(int projectId);

        Task<QuoteReadDto?> Get(int id);

        /// <summary>A new draft for the project (with the flat price as first line for flat price projects).</summary>
        Task<QuoteReadDto> CreateDraft(int projectId);

        Task<QuoteReadDto> UpdateDraft(int id, QuoteUpdateDto dto);

        Task DeleteDraft(int id);

        /// <summary>Issues the draft: next quote number, valid until, business data frozen - status Sent.</summary>
        Task<QuoteReadDto> Issue(int id, InvoiceDateDto dto);

        /// <summary>The client's answer: Accepted, Declined (or back to Sent).</summary>
        Task<QuoteReadDto> SetStatus(int id, QuoteStatusDto dto);
    }
}
