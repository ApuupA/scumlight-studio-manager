namespace StudioManager.DataLayer.Models.FinanceModels
{
    public enum QuoteStatus
    {
        /// <summary>Not issued yet - can be changed or deleted.</summary>
        Draft,

        /// <summary>Issued with a number and sent to the client - waiting for an answer.</summary>
        Sent,

        Accepted,
        Declined,

        /// <summary>An invoice was made from it.</summary>
        Invoiced
    }
}
