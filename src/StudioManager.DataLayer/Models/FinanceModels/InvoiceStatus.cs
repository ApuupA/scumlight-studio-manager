namespace StudioManager.DataLayer.Models.FinanceModels
{
    public enum InvoiceStatus
    {
        /// <summary>Not issued yet - can be changed or deleted.</summary>
        Draft,

        /// <summary>Issued with a number - locked. Paid / open / overdue comes from the payments.</summary>
        Issued,

        /// <summary>Cancelled by a cancellation invoice.</summary>
        Cancelled
    }
}
