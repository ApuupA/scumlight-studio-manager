using StudioManager.DataLayer.Models.FinanceModels;

namespace StudioManager.DataLayer.DTO.Create
{
    /// <summary>
    /// The client's answer to a sent quote: Accepted or Declined (or back to Sent).
    /// </summary>
    public class QuoteStatusDto
    {
        public QuoteStatus Status { get; set; }
    }
}
